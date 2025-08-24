using System.Security.Claims;
using FinanceWebApp.Data.Service;
using FinanceWebApp.Models;
using FinanceWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanceWebApp.Controllers;
public class CategoriesController: Controller
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
    
    //POST
    [HttpPost]
    public async Task<IActionResult> Create(Category category)
    {
        if (ModelState.IsValid)
        {
            await _categoryService.Add(category);
            return RedirectToAction(nameof(Index));
        }
        return View();// it returns empty CreateModel instead our view
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var category = new Category
            {
                Name = model.Name,
                Type = model.Type,
                ParentCategoryId = model.ParentCategoryId,
                UserId = userId
            };

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
}