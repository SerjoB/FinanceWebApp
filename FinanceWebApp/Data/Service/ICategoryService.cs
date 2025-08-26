using FinanceWebApp.Models;

namespace FinanceWebApp.Data.Service;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAll();
    Task Add(Category category);

    Task<ApplicationUser?> GetCurrentUserAsync();
}