using FinanceWebApp.Data.Service.Models;
using FinanceWebApp.Models;
using FinanceWebApp.Models.Enums;

namespace FinanceWebApp.Data.Service;

public interface ITransactionImportService
{
    /// <summary>
    /// Reads transactions from the given file (CSV or XLSX).
    /// Returns raw parsed TransactionImportModel before categorization.
    /// </summary>
    Task<List<TransactionImportModel>> ImportAsync(Stream fileStream, string fileName);
}