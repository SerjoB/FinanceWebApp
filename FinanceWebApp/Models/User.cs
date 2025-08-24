namespace FinanceWebApp.Models;

public class User
{
    public int UserId { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Category>? Categories { get; set; }
    public ICollection<Transaction>? Transactions { get; set; }
    public ICollection<CategorizationRule>? Rules { get; set; }
}