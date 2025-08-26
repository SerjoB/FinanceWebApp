using FinanceWebApp.Models;
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

    public async Task Add(Category category)
    {
        _context.Categories.Add(category);
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