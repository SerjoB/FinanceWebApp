using FinanceWebApp.Models;
using FinanceWebApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

    public async Task<Category?> GetCategoryByIdAsync(int id, QueryOptions<Category> options)
    {
        IQueryable<Category> query = _context.Set<Category>();
        if (options.HasWhere())
        {
            query = query.Where(options.Where);
        }

        foreach(var include in options.GetIncludes())
        {
            query = query.Include(include);
        }
        
        var user = await GetCurrentUserAsync();
        return await query
            .FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == user.Id);
    }

    public async Task Add(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
    }

    public async Task Update(CategoryCreateViewModel model ,int categoryId)
    {
        var category = await GetCategoryByIdAsync(categoryId, new QueryOptions<Category>());
        category.Name = model.Name;
        category.Type = model.Type;
        category.ParentCategoryId = model.ParentCategoryId;

        _context.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var category = await GetCategoryByIdAsync(id, new QueryOptions<Category>());
        if (category == null)
            return;
        _context.Remove(category);
        await _context.SaveChangesAsync();
    }

    public async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
            return null;

        return await _userManager.GetUserAsync(user);
    }
    
}