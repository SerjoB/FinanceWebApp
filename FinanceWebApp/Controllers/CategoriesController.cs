using System.ComponentModel.DataAnnotations;
using FinanceWebApp.Data.Service;
using FinanceWebApp.Models;
using FinanceWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FinanceWebApp.Controllers;
// TODO Change flat list of categories to a dropdown tree
// TODO Show user friendly messages instead just errors
[Authorize]
public class CategoriesController: Controller
{
    readonly ICategoryService _categoryService;
    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }
    public async Task<IActionResult> Index()
    { 
        var categories = await _categoryService.GetAll();
        return View(categories);
    }
    //  GET
    public async Task<IActionResult> Create()   //TODO: Add Validation Parent-Type. So if parent of type "Expenses" category must be "Expenses"
    {
        var categories = await _categoryService.GetAll();
        var model = new CategoryCreateViewModel
        {
            CategoryTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Expense", Text = "Expense" },
                new SelectListItem { Value = "Income", Text = "Income" }
            },
            ParentCategories = categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.Name
            })
        };

        return View(model);
    }
    
    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryCreateViewModel model)  // TODO: When parent category is selected force Type of category and disable Type field
    {
        var category =  await CategoryValidator(model);
        if (category != null)
        {
            var result = await _categoryService.Add(category);
            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage;
            }
            return RedirectToAction(nameof(Index));
        }
        
        // If validation fails, reload dropdowns
        model.CategoryTypes = new List<SelectListItem>
        {
            new SelectListItem { Value = "Expense", Text = "Expense" },
            new SelectListItem { Value = "Income", Text = "Income" }
        };
        var categories = await _categoryService.GetAll();
        model.ParentCategories = categories.Select(c => new SelectListItem
        {
            Value = c.CategoryId.ToString(),
            Text = c.Name
        });

        return View(model);
    }
    
    //  GET
    public async Task<IActionResult> Update(int id)
    {
        var categories = await _categoryService.GetAll();
        var category = await _categoryService.GetCategoryByIdAsync(id, new QueryOptions<Category>());
        if(category == null)
            return NotFound();
        var descendantsIds = await _categoryService.GetAllDescendantsIdsAsync(category.CategoryId);
        var model = new CategoryCreateViewModel
        {
            CategoryTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Expense", Text = "Expense" },
                new SelectListItem { Value = "Income", Text = "Income" }
            },
            ParentCategories = categories.Where(c => c.CategoryId != id && !descendantsIds.Contains(c.CategoryId)).Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.Name
            }),
            Name = category.Name,
            Type = category.Type,
            ParentCategoryId = category.ParentCategoryId
        };

        return View(model);
    }
    
    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(CategoryCreateViewModel model, int id)
    {
        var user = await _categoryService.GetCurrentUserAsync();
        if (await _categoryService.CategoryExistsAsync(model.Name, user.Id, model.ParentCategoryId, id))
        {
            ModelState.AddModelError("Name", "Category name must be unique.");
        }
        if (ModelState.IsValid)
        {
            var result = await _categoryService.Update(model, id);
            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage;
            }
            return RedirectToAction(nameof(Index));
        }
        
        
        // If validation fails, reload dropdowns
        var categories = await _categoryService.GetAll();
        var currentCategory = await _categoryService.GetCategoryByIdAsync(id, new QueryOptions<Category>());
        if (currentCategory == null)
            return NotFound();
        var descendantsIds = await _categoryService.GetAllDescendantsIdsAsync(currentCategory.CategoryId);
        var currentModel = new CategoryCreateViewModel
        {
            CategoryTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Expense", Text = "Expense" },
                new SelectListItem { Value = "Income", Text = "Income" }
            },
            ParentCategories = categories.Where(c => c.CategoryId != id && !descendantsIds.Contains(id)).Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.Name
            }),
            Name = currentCategory.Name,
            Type = currentCategory.Type
        };

        return View(currentModel);
    }
    
    //GET
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id, new QueryOptions<Category>() {Includes = "ParentCategory, Subcategories"});
        if (category == null)
            return NotFound();
        return View(category);
    }
    
    // POST
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, string mode)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id, new QueryOptions<Category>() {Includes = "Subcategories"});

        if (id == 1) 
        {
            TempData["ErrorMessage"] = "The 'Uncategorized' category cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }
        if (category == null)
            return NotFound();

        switch (mode)
        {
            case "orphan":
            {
                foreach (var child in category.Subcategories)
                {
                    child.ParentCategoryId = 1; // or reassign to "Uncategorized"
                }
                //
                // category.Subcategories.Clear();
                var result = await _categoryService.Delete(category);
                if (!result.Success)
                {
                    TempData["Error"] = result.ErrorMessage;
                }
                break;
            }
            case "cascade":
                // delete all children + parent
                await DeleteCategoryTree(category);
                break;
        }
        return RedirectToAction(nameof(Index));
    }
    
    private async Task DeleteCategoryTree(Category category)
    {
        for (var i = 0; i < category.Subcategories.Count; i++)
        {
            await DeleteCategoryTree(category.Subcategories.ElementAt(i));
        }

        try
        { 
            await _categoryService.Delete(category);
        }
        catch (InvalidOperationException  ex)
        {
            TempData["Error"] = ex.Message; 
        }
        catch (DbUpdateException ex)
        {
            // Database error (constraint violation, etc.)
            TempData["Error"] = "Unable to delete category due to database constraints.";
            Console.WriteLine(ex);
            // log ex for developers
        }
        catch (Exception ex)
        {
            // Unexpected errors
            TempData["Error"] = "Unexpected error occurred.";
            Console.WriteLine(ex);
            // log ex for developers
        }
    }
    

    private async Task<Category?> CategoryValidator(CategoryCreateViewModel model)  // TODO: should look into it. I am not sure i need it anymore. Possibly just change it for better look
    {
        var user = await _categoryService.GetCurrentUserAsync();
        if (await _categoryService.CategoryExistsAsync(model.Name, user.Id, model.ParentCategoryId))
        {
            ModelState.AddModelError("Name", "Category name must be unique.");
        }
        if (!ModelState.IsValid) return null;
        var category = new Category
        {
            Name = model.Name,
            Type = model.Type,
            ParentCategoryId = model.ParentCategoryId,
            UserId = user!.Id
        };
       

        var context = new ValidationContext(category);
        var results = new List<ValidationResult>();
        return Validator.TryValidateObject(category, context, results) ? category : null;
    }
}