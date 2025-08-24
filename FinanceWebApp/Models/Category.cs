namespace FinanceWebApp.Models;

public class Category
{
    public int CategoryId { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!; // "Income" or "Expense"
    public int? ParentCategoryId { get; set; }

    public User User { get; set; } = null!;
    public Category? ParentCategory { get; set; }
    public ICollection<Category> Subcategories { get; set; } = new List<Category>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}