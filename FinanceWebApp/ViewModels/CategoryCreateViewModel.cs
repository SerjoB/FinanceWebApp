using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanceWebApp.ViewModels;

public class CategoryCreateViewModel
{
    public string Name { get; set; }
    public string Type { get; set; }
    public int? ParentCategoryId { get; set; }  // optional, because not all categories have a parent

    // Dropdowns
    [ValidateNever]
    public IEnumerable<SelectListItem> CategoryTypes { get; set; }
    [ValidateNever]
    public IEnumerable<SelectListItem> ParentCategories { get; set; }
}