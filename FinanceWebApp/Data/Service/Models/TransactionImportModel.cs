namespace FinanceWebApp.Data.Service.Models;

public class TransactionImportModel
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? Source { get; set; }
    public string? CategoryName { get; set; }
}