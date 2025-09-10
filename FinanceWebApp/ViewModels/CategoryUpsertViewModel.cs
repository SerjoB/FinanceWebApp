using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanceWebApp.ViewModels;

public class CategoryUpsertViewModel
{
    public string Name { get; set; }
    public string Type { get; set; }
    public int ParentCategoryId { get; set; } 

    // Dropdowns
    [ValidateNever]
    public IEnumerable<SelectListItem> CategoryTypes { get; set; }
    [ValidateNever]
    public IEnumerable<ParentCategoryOption> ParentCategories { get; set; }
}

public class ParentCategoryOption
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; } // "Income" or "Expense"
}