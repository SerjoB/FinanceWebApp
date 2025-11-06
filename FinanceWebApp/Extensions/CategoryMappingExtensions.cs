using FinanceWebApp.Models;
using FinanceWebApp.Models.DTOs;
using FinanceWebApp.Models.Enums;
using FinanceWebApp.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanceWebApp.Extensions;

public static class CategoryMappingExtensions
{
    public static CategoryTreeViewModel ToTreeViewModel(this Category category)
    {
        return new CategoryTreeViewModel
        {
            CategoryId = category.Id,
            Name = category.Name,
            Type = category.Type,
            Subcategories = category.Subcategories
                .Select(c => c.ToTreeViewModel())
                .ToList()
        };
    }

    public static Category ToEntity(this CategoryUpsertViewModel model, int userId)
    {
        return new Category
        {
            Name = model.Name,
            Type = model.Type,
            ParentCategoryId = model.ParentCategoryId,
            UserId = userId
        };
    }

    public static CategoryUpsertViewModel ToCreateViewModel(this Category category, IEnumerable<Category> categories, List<int> descendantsIds)
    {
        return new CategoryUpsertViewModel
        {
            Name = category.Name,
            Type = category.Type,
            ParentCategoryId = category.ParentCategoryId,
            CategoryTypes = GetCategoriesTypes(),
            ParentCategories = GetParentCategoriesNoChildren(category.Id,categories, descendantsIds)
        };
    }

    public static CategoryUpsertViewModel NewCreateViewModel(IEnumerable<Category> categories)
    {
        return new CategoryUpsertViewModel
        {
            CategoryTypes = GetCategoriesTypes(),
            ParentCategories = GetParentCategories(categories)
        };
    }
    
    public static CategorySelectItem ToSelectItem(this Category category)
    {
        return new CategorySelectItem
        {
            Name = category.Name,
            Id = category.Id,
            Type = category.Type
        };
    }

    public static IEnumerable<ParentCategoryOption> GetParentCategories(IEnumerable<Category> categories)
    {
        var parentCategories = categories.Select(c => new ParentCategoryOption
        {
            Id = c.Id,
            Name = c.Name,
            Type = c.Type
        });
        return parentCategories;
    }
    
    public static IEnumerable<ParentCategoryOption> GetParentCategoriesNoChildren(int categoryId,IEnumerable<Category> categories, List<int> descendantsIds)
    {
        var parentCategories = categories.Where(c => c.Id != categoryId && !descendantsIds.Contains(c.Id)).Select(c => new ParentCategoryOption
        {
            Id = c.Id,
            Name = c.Name,
            Type = c.Type
        });
        return parentCategories;
    }

    public static IEnumerable<SelectListItem> GetCategoriesTypes()
    {
        var categoryTypes = Enum.GetValuesAsUnderlyingType<TransactionType>()
            .Cast<TransactionType>()
            .Select(t => new SelectListItem
            {
                Value = t.ToString(),
                Text = t.ToString()
            });
        return categoryTypes;
    }
}
