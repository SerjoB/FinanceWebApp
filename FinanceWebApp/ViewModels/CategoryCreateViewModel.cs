using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanceWebApp.ViewModels;

public class CategoryCreateViewModel
{
    public string Name { get; set; }
    public string Type { get; set; }
    public int? ParentCategoryId { get; set; }  // optional, because not all categories have a parent

    // Dropdowns
    public IEnumerable<SelectListItem> CategoryTypes { get; set; }
    public IEnumerable<SelectListItem> ParentCategories { get; set; }
}