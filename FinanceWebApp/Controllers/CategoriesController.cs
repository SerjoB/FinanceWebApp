using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using FinanceWebApp.Data.Service;
using FinanceWebApp.Models;
using FinanceWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FinanceWebApp.Controllers;
[Authorize]
public class CategoriesController: Controller   //TODO find out how to handle possible null data from db (variables) and fix it everywhere
{
    ICategoryService  _categoryService;
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
    public async Task<IActionResult> Create()
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
    public async Task<IActionResult> Create(CategoryCreateViewModel model)
    {
        var category =  await CategoryValidator(model);
        if (category != null)
        {
            await _categoryService.Add(category);
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
    
    
    public async Task<IActionResult> Update(int id)
    {
        var categories = await _categoryService.GetAll();
        var category = categories.FirstOrDefault(c => c.CategoryId == id);
        
        var model = new CategoryCreateViewModel
        {
            CategoryTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Expense", Text = "Expense" },
                new SelectListItem { Value = "Income", Text = "Income" }
            },
            ParentCategories = categories.Where(c => c.CategoryId != id).Select(c => new SelectListItem
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
        if (ModelState.IsValid)
        {
            await _categoryService.Update(model, id);
            return RedirectToAction(nameof(Index));
        }
        
        
        // If validation fails, reload dropdowns
        var categories = await _categoryService.GetAll();
        var currentCategory = await _categoryService.GetCategoryByIdAsync(id, new QueryOptions<Category>());
        
        var currentModel = new CategoryCreateViewModel
        {
            CategoryTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Expense", Text = "Expense" },
                new SelectListItem { Value = "Income", Text = "Income" }
            },
            ParentCategories = categories.Where(c => c.CategoryId != id).Select(c => new SelectListItem
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

        if (category == null)
            return NotFound();

        switch (mode)
        {
            case "orphan":
            {
                foreach (var child in category.Subcategories)
                {
                    child.ParentCategoryId = null; // or reassign to "Uncategorized"
                }

                category.Subcategories.Clear();

                await _categoryService.Delete(id);
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
            await _categoryService.Delete(category.CategoryId);
        }
        catch (InvalidOperationException  ex)
        {
            TempData["Error"] = ex.Message; 
        }
        catch (DbUpdateException ex)
        {
            // Database error (constraint violation, etc.)
            TempData["Error"] = "Unable to delete category due to database constraints.";
            // log ex for developers
        }
        catch (Exception ex)
        {
            // Unexpected errors
            TempData["Error"] = "Unexpected error occurred.";
            // log ex for developers
        }
    }
    

    private async Task<Category?> CategoryValidator(CategoryCreateViewModel model)  // TODO: should look into it. I am not sure i need it anymore. Possibly just change it for better look
    {
        if (!ModelState.IsValid) return null;
        var user = await _categoryService.GetCurrentUserAsync();
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