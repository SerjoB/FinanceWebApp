using FinanceWebApp.Data.Service.Models;
using FinanceWebApp.Models;
using FinanceWebApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FinanceWebApp.Data.Service;

public class CategoryService: ICategoryService
{
    private readonly FinanceAppDbContext _context;
    private IRepository<Category> _repository;

    public CategoryService(FinanceAppDbContext context,
        IRepository<Category> repository)
    {
        _context = context;
        _repository = repository;
    }
    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        var user = await GetCurrentUserAsync();
        var categories = await _context.Categories.Include(c => c.ParentCategory)
            .Where(c => c.UserId == user.Id)
            .ToListAsync(); 
        return categories;  
    }

    public async Task<IEnumerable<Category>> GetAllWithOptionsAsync(QueryOptions<Category> options)
        => await _repository.GetAllWithOptionsAsync(options);

    public async Task<Category?> GetCategoryByIdAsync(int id, QueryOptions<Category> options)
        => await _repository.GetEntityByIdAsync(id, options);

    public async Task<Category?> GetCategoryAsync(QueryOptions<Category> options)
        => await _repository.GetEntityAsync(options);


    public async Task<List<int>?> GetAllDescendantsIdsAsync(int categoryId) // TODO: this doesn't work good with big hierarchies, so it must be replaced with a non-recursive query using a CTE or caching the tree in memory
    {
        List<int> result = new List<int>();
        var currentCategory = await GetCategoryByIdAsync(categoryId, new QueryOptions<Category>() {Includes = "Subcategories"});
        if(currentCategory == null)
            return result;
        
        foreach (var child in currentCategory.Subcategories)
        {
            result.Add(child.Id);
            result.AddRange(await GetAllDescendantsIdsAsync(child.Id));
        }
        return result;
    }

    public async Task<ServiceResult> Add(Category category)
        =>  await _repository.Add(category);

    public async Task<ServiceResult> Update(CategoryUpsertViewModel model, int categoryId)
    {
        var category = await GetCategoryByIdAsync(categoryId, new QueryOptions<Category>());
        if (category == null)
            return ServiceResult.Fail("Category not found.");
        category.Name = model.Name;
        category.Type = model.Type;
        category.ParentCategoryId = model.ParentCategoryId;
        return await _repository.Update(category);
    }

    public async Task<ServiceResult> Delete(Category category)
        => await _repository.Delete(category);

    public async Task<bool> CategoryExistsAsync(string name, int userId, int parentCategoryId, int? excludeCategoryId = null)
    {
        IQueryable<Category> query = _context.Categories
            .Where(c => c.UserId == userId && c.Name == name && c.ParentCategoryId == parentCategoryId);

        if (excludeCategoryId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCategoryId.Value);
        }

        return await query.AnyAsync();
    }
    public async Task<ApplicationUser?> GetCurrentUserAsync() 
        => await _repository.GetCurrentUserAsync();
    
    
}