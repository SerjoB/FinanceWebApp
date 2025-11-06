using FinanceWebApp.Data.Service.Models;
using FinanceWebApp.Models;
using FinanceWebApp.Models.DTOs;

namespace FinanceWebApp.Data.Service;

public interface ITransactionImportService
{
    Task<List<TransactionImportModel>> ImportAsync(Stream fileStream, string fileName);

    Task<ServiceResult> SaveTransactionsAsync(List<TransactionImportModel> importedTransactions);
    Task<List<CategorySelectItem>> GetAllCategorySelectItemsAsync();
}