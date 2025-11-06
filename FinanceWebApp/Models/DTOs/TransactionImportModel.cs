using FinanceWebApp.Models.Enums;

namespace FinanceWebApp.Models.DTOs;

public class TransactionImportModel
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public TransactionType  TransactionType { get; set; }
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? Source { get; set; }
    public string? ParsedCategoryName { get; set; }
    public string? CategoryName { get; set; }
}