namespace FinanceWebApp.Models;

public class Transaction
{
    public int TransactionId { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public int? CategoryId { get; set; }
    public string? Description { get; set; }
    public string? Source { get; set; }

    public User User { get; set; } = null!;
    public Category? Category { get; set; }
}