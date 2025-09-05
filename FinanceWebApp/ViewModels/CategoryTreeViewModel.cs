using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanceWebApp.ViewModels;

public class CategoryTreeViewModel
{
    public int CategoryId { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public int ParentCategoryId { get; set; } 
    
    [ValidateNever]
    public List<CategoryTreeViewModel> Subcategories { get; set; } = new();
}