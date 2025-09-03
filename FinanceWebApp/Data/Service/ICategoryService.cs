using FinanceWebApp.Data.Service.Models;
using FinanceWebApp.Models;
using FinanceWebApp.ViewModels;

namespace FinanceWebApp.Data.Service;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAll();
    Task<IEnumerable<Category>> GetAllWithOptions(QueryOptions<Category> options);
    
    Task<Category?> GetCategoryByIdAsync(int id, QueryOptions<Category> options);
    Task<Category?> GetCategoryAsync(QueryOptions<Category> options);
    Task<bool> CategoryExistsAsync(string name, int userId, int parentCategoryId, int? excludeCategoryId = null);
    Task<List<int>?> GetAllDescendantsIdsAsync(int categoryId);

    Task<ServiceResult> Add(Category category);
    Task<ServiceResult> Update(CategoryCreateViewModel model, int categoryId);
    Task<ServiceResult> Delete(Category category);

    Task<ApplicationUser?> GetCurrentUserAsync();
}