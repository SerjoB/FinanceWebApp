using FinanceWebApp.Data.Service.Models;
using FinanceWebApp.Models.DTOs;

namespace FinanceWebApp.ViewModels;

public class TransactionImportPreviewViewModel
{
    public List<TransactionImportModel> Transactions { get; set; } = new();
    public required IEnumerable<CategorySelectItem> AllCategories { get; set; }
    public int TotalCount => Transactions.Count;
    public decimal ExpensesTotalAmount => Transactions.Sum(t => t.Amount < 0 ? t.Amount : 0);
    public decimal IncomeTotalAmount => Transactions.Sum(t => t.Amount > 0 ? t.Amount : 0);
}