using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using FinanceWebApp.Data.Service;
using FinanceWebApp.Models;
using FinanceWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanceWebApp.Controllers;
[Authorize]
public class CategoriesController: Controller
{
    ICategoryService  _categoryService;
    private readonly UserManager<ApplicationUser> _userManager;
    public CategoriesController(ICategoryService categoryService, UserManager<ApplicationUser> userManager)
    {
        _categoryService = categoryService;
        _userManager = userManager;
    }
    public async Task<IActionResult> Index()
    { 
        var categories = await _categoryService.GetAll();
        

        return View(categories);
    }
    
    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            var category = new Category
            {
                Name = model.Name,
                Type = model.Type,
                ParentCategoryId = model.ParentCategoryId,
                UserId = user.Id
            };
            
            var context = new ValidationContext(category);
            var results = new List<ValidationResult>();
            if (Validator.TryValidateObject(category, context, results, true))
            {
                await _categoryService.Add(category);
                return RedirectToAction(nameof(Index));
            }
            
            foreach (var validationResult in results)
            {
                if (validationResult.ErrorMessage != null)
                    ModelState.AddModelError("", validationResult.ErrorMessage);
            }
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