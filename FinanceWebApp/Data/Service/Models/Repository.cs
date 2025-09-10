using FinanceWebApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FinanceWebApp.Data.Service.Models;

public class Repository<T> : IRepository<T> where T: class, IUserOwnedEntity
{
    private FinanceAppDbContext _context { get; set; }
    private DbSet<T> _dbSet { get; set; }
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Repository(FinanceAppDbContext context,
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _dbSet = context.Set<T>();
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
            return null;
        return await _userManager.GetUserAsync(user);
    }

    public async Task<IEnumerable<T>> GetAllWithOptionsAsync(QueryOptions<T> options)
    {
        IQueryable<T> query = _dbSet;
        if (options.HasFilter())
        {
            query = query.Where(options.Filter!);
        }

        foreach(var include in options.GetIncludes())
        {
            query = query.Include(include);
        }
        
        var user = await GetCurrentUserAsync();
        if(user == null)
            return null;
 
        return await query
            .Where(c => c.UserId == user.Id)
            .ToListAsync(); 
    }

    public async Task<T?> GetEntityByIdAsync(int id, QueryOptions<T> options)
    {
        options.Filter = c => c.Id == id;
        return await GetEntityAsync(options);
    }

    public async Task<T?> GetEntityAsync(QueryOptions<T> options)
    {
        IQueryable<T> query = _dbSet;
        if (options.HasFilter())
        {
            query = query.Where(options.Filter!);
        }

        foreach(var include in options.GetIncludes())
        {
            query = query.Include(include);
        }
        
        var user = await GetCurrentUserAsync();
        if(user == null)
            return null;
        
        return await query
            .FirstOrDefaultAsync(c => c.UserId == user.Id);
    }

    public async Task<ServiceResult> Add(T entity)
    {
        try
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return ServiceResult.Ok();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex);
            return ServiceResult.Fail("Database error occurred while creating entity.");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Unexpected error: {ex.Message}");
        }
    }

    public async Task<ServiceResult> Update(T entity)
    {
        try
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
            return ServiceResult.Ok();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex);
            return ServiceResult.Fail("Database error occurred while Updating entity.");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Unexpected error: {ex.Message}");
        }
    }

    public async Task<ServiceResult> Delete(T entity)
    {
        try
        {
            _context.Remove(entity);
            await _context.SaveChangesAsync();
            return ServiceResult.Ok();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex);
            return ServiceResult.Fail("Database error occurred while Deleting entity.");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Unexpected error: {ex.Message}");
        }
    }
}