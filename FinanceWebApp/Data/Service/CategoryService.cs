using FinanceWebApp.Data.Service.Models;
using FinanceWebApp.Models;
using FinanceWebApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FinanceWebApp.Data.Service;

public class CategoryService: ICategoryService
{
    private FinanceAppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CategoryService(FinanceAppDbContext context,
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<IEnumerable<Category>> GetAll()
    {
        var user = await GetCurrentUserAsync();
        var categories = await _context.Categories.Include(c => c.ParentCategory)
            .Where(c => c.UserId == user.Id)
            .ToListAsync(); 
        return categories;  
    }
    
    public async Task<IEnumerable<Category>> GetAllWithOptions(QueryOptions<Category> options)
    {
        IQueryable<Category> query = _context.Set<Category>();
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
 
        var categories = await query
            .Where(c => c.UserId == user.Id)
            .ToListAsync(); 
        return categories;  
    }

    public async Task<Category?> GetCategoryByIdAsync(int id, QueryOptions<Category> options)
    {
        options.Filter = c => c.CategoryId == id;
        return await GetCategoryAsync(options);
    }

    public async Task<Category?> GetCategoryAsync(QueryOptions<Category> options)
    {
        IQueryable<Category> query = _context.Set<Category>();
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

    public async Task<List<int>?> GetAllDescendantsIdsAsync(int categoryId) // TODO: this doesn't work good with big hierarchies, so it must be replaced with a non-recursive query using a CTE or caching the tree in memory
    {
        List<int> result = new List<int>();
        var currentCategory = await GetCategoryByIdAsync(categoryId, new QueryOptions<Category>() {Includes = "Subcategories"});
        if(currentCategory == null)
            return result;
        
        foreach (var child in currentCategory.Subcategories)
        {
            result.Add(child.CategoryId);
            result.AddRange(await GetAllDescendantsIdsAsync(child.CategoryId));
        }
        return result;
    }

    public async Task<bool> CategoryExistsAsync(string name, int userId, int parentCategoryId, int? excludeCategoryId = null)
    {
        IQueryable<Category> query = _context.Categories
            .Where(c => c.UserId == userId && c.Name == name && c.ParentCategoryId == parentCategoryId);

        if (excludeCategoryId.HasValue)
        {
            query = query.Where(c => c.CategoryId != excludeCategoryId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<ServiceResult> Add(Category category)
    {
        try
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return ServiceResult.Ok();
            
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex);
            return ServiceResult.Fail("Database error occurred while creating category.");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Unexpected error: {ex.Message}");
        }
        
    }

    public async Task<ServiceResult> Update(CategoryCreateViewModel model ,int categoryId)
    {
        var category = await GetCategoryByIdAsync(categoryId, new QueryOptions<Category>());
        if (category == null)
            return ServiceResult.Fail("Category not found.");
        category.Name = model.Name;
        category.Type = model.Type;
        category.ParentCategoryId = model.ParentCategoryId;

        try
        {
            _context.Update(category);
            await _context.SaveChangesAsync();
            return ServiceResult.Ok();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex);
            return ServiceResult.Fail("Database error occurred while Updating category.");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Unexpected error: {ex.Message}");
        }
    }

    public async Task<ServiceResult> Delete(Category category)
    {
        try
        {
            _context.Remove(category);
            await _context.SaveChangesAsync();
            return ServiceResult.Ok();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex);
            return ServiceResult.Fail("Database error occurred while Deleting category.");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Unexpected error: {ex.Message}");
        }
    }

    public async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
            return null;

        return await _userManager.GetUserAsync(user);
    }
    
}