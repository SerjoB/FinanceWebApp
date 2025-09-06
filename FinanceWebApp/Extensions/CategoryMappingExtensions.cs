using FinanceWebApp.Models;
using FinanceWebApp.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanceWebApp.Extensions;

public static class CategoryMappingExtensions
{
    public static CategoryTreeViewModel ToTreeViewModel(this Category category)
    {
        return new CategoryTreeViewModel
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            Type = category.Type,
            Subcategories = category.Subcategories
                .Select(c => c.ToTreeViewModel())
                .ToList()
        };
    }

    public static Category ToEntity(this CategoryCreateViewModel model, int userId)
    {
        return new Category
        {
            Name = model.Name,
            Type = model.Type,
            ParentCategoryId = model.ParentCategoryId,
            UserId = userId
        };
    }

    public static CategoryCreateViewModel ToCreateViewModel(this Category category, IEnumerable<Category> categories, List<int> descendantsIds)
    {
        return new CategoryCreateViewModel
        {
            Name = category.Name,
            Type = category.Type,
            ParentCategoryId = category.ParentCategoryId,
            CategoryTypes = GetCategoriesTypes(),
            ParentCategories = GetParentCategoriesNoChildren(category.CategoryId,categories, descendantsIds)
        };
    }

    public static CategoryCreateViewModel NewCreateViewModel(IEnumerable<Category> categories)
    {
        return new CategoryCreateViewModel
        {
            CategoryTypes = GetCategoriesTypes(),
            ParentCategories = GetParentCategories(categories)
        };
    }

    public static IEnumerable<ParentCategoryOption> GetParentCategories(IEnumerable<Category> categories)
    {
        var parentCategories = categories.Select(c => new ParentCategoryOption
        {
            Id = c.CategoryId,
            Name = c.Name,
            Type = c.Type
        });
        return parentCategories;
    }
    
    public static IEnumerable<ParentCategoryOption> GetParentCategoriesNoChildren(int categoryId,IEnumerable<Category> categories, List<int> descendantsIds)
    {
        var parentCategories = categories.Where(c => c.CategoryId != categoryId && !descendantsIds.Contains(c.CategoryId)).Select(c => new ParentCategoryOption
        {
            Id = c.CategoryId,
            Name = c.Name,
            Type = c.Type
        });
        return parentCategories;
    }

    public static IEnumerable<SelectListItem> GetCategoriesTypes()
    {
        var categoryTypes = Enum.GetValuesAsUnderlyingType<CategoryType>()
            .Cast<CategoryType>()
            .Select(t => new SelectListItem
            {
                Value = t.ToString(),
                Text = t.ToString()
            });
        return categoryTypes;
    }
}
