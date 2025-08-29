using FinanceWebApp.Models;
using FinanceWebApp.ViewModels;

namespace FinanceWebApp.Data.Service;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAll();
    
    Task<Category?> GetCategoryByIdAsync(int id, QueryOptions<Category> options);
    Task Add(Category category);
    Task Update(CategoryCreateViewModel model, int categoryId);
    Task Delete(int id);

    Task<ApplicationUser?> GetCurrentUserAsync();
}