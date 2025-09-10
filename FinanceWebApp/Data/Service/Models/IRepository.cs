using FinanceWebApp.Models;

namespace FinanceWebApp.Data.Service.Models;

public interface IRepository<T> where T: class, IUserOwnedEntity
{
    Task<ApplicationUser?> GetCurrentUserAsync();
    Task<IEnumerable<T>> GetAllWithOptionsAsync(QueryOptions<T> options);
    Task<T?> GetEntityByIdAsync(int id, QueryOptions<T> options);
    Task<T?> GetEntityAsync(QueryOptions<T> options);
    Task<ServiceResult> Add(T entity);
    Task<ServiceResult> Update(T entity);
    Task<ServiceResult> Delete(T entity);
}