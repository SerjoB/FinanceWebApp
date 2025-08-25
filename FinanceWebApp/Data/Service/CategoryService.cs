using FinanceWebApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FinanceWebApp.Data.Service;

public class CategoryService: ICategoryService
{
    private FinanceAppDbContext _context;


    public CategoryService(FinanceAppDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<Category>> GetAll()
    {
        var categories = await _context.Categories.Include(c => c.ParentCategory).ToListAsync();   //TODO make them to be selected by user
        return categories;  
    }

    public async Task Add(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
    }

    
}