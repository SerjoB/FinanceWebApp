using System.ComponentModel.DataAnnotations;

namespace FinanceWebApp.Models;

public class Category:IUserOwnedEntity
{
    [Range(0, int.MaxValue, ErrorMessage = "Id must be greater than 0")]
    public int Id { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Id must be greater than 0")]
    public int UserId { get; set; }
    [Required, StringLength(50)]
    public string Name { get; set; } = null!;
    [Required]
    public string Type { get; set; } = null!; // "Income" or "Expense"
    public int ParentCategoryId { get; set; }

    public ApplicationUser ApplicationUser { get; set; } = null!;
    public Category? ParentCategory { get; set; }
    public ICollection<Category> Subcategories { get; set; } = new List<Category>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}