using FinanceWebApp.Data.Service.Models;
using FinanceWebApp.Models;
using FinanceWebApp.ViewModels;

namespace FinanceWebApp.Data.Service;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAll();
    
    Task<Category?> GetCategoryByIdAsync(int id, QueryOptions<Category> options);
    Task<ServiceResult> Add(Category category);
    Task<ServiceResult> Update(CategoryCreateViewModel model, int categoryId);
    Task<ServiceResult> Delete(Category category);

    Task<ApplicationUser?> GetCurrentUserAsync();
}