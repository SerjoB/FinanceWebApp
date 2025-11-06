using FinanceWebApp.Data.Service.Models;
using FinanceWebApp.Models;
using FinanceWebApp.Models.DTOs;
using FinanceWebApp.Models.Enums;

namespace FinanceWebApp.Data.Service;

public interface ITransactionService
{
    /// <summary>
    /// Reads transactions from the given file (CSV or XLSX).
    /// Returns raw parsed TransactionImportModel before categorization.
    /// </summary>
    Task<ServiceResult> AddRange(List<Transaction> transaction);

    Task<ApplicationUser?> GetCurrentUserAsync();
}