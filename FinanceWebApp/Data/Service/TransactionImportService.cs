using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using ExcelDataReader;
using FinanceWebApp.Configurations;
using FinanceWebApp.Data.Service.Models;
using FinanceWebApp.Extensions;
using FinanceWebApp.Models;
using FinanceWebApp.Models.DTOs;
using FinanceWebApp.Models.Enums;
using FinanceWebApp.Utilities;
using Microsoft.Extensions.Options;

namespace FinanceWebApp.Data.Service;

public class TransactionImportService: ITransactionImportService
{
    ICategoryService _categoryService;
    ITransactionService _transactionService;
    private readonly CategorySettings _settings;

    public TransactionImportService(ICategoryService categoryService,
        ITransactionService  transactionService,
        IOptions<CategorySettings> options)
    {
        _categoryService = categoryService;
        _transactionService = transactionService;
        _settings = options.Value;
    }
    
    
    public async Task<List<TransactionImportModel>> ImportAsync(Stream fileStream, string fileName)
    {
        var fileType = DetectFileType(fileName);

        return fileType switch
        {
            FileType.Csv => ParserUtility.ParseCsv(fileStream),
            FileType.Excel =>await AssignExistingCategories(ParserUtility.ParseExcel(fileStream)),
            _ => throw new NotSupportedException("Unsupported file type.")
        };
    }

    private FileType DetectFileType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".csv" => FileType.Csv,
            ".xls" or ".xlsx" => FileType.Excel,
            _ => FileType.Unknown
        };
    }
    private async Task<List<TransactionImportModel>> AssignExistingCategories(List<TransactionImportModel> models)
    {
        foreach (var model in models)
        {
            var cat = await _categoryService.GetCategoryByNameAndTypeAsync(model.ParsedCategoryName, model.TransactionType.ToString());
            model.CategoryId = cat?.Id ?? _settings.UncategorizedCategoryId;
            model.CategoryName = model.CategoryId == 1 ? _settings.UncategorizedCategoryName : model.ParsedCategoryName;
        }
        return models;
    }

    public async Task<ServiceResult> SaveTransactionsAsync(List<TransactionImportModel> importedTransactions)
    {
        var transactions = await TransactionImportModelToEntityAsync(importedTransactions);
        return await _transactionService.AddRange(transactions);
    }
    
    private async Task<List<Transaction>> TransactionImportModelToEntityAsync(List<TransactionImportModel> importedTransactions)
    {
        List<Transaction> entities = [];
        if (importedTransactions == null || importedTransactions.Count == 0)
            return entities;

        var user = await _transactionService.GetCurrentUserAsync();
        entities = importedTransactions.Select(t => new Transaction
        {
            Date = DateTime.SpecifyKind(t.Date, DateTimeKind.Utc),
            Amount = t.Amount,
            Description = t.Description,
            CategoryId = t.CategoryId,
            UserId = user.Id,
            Source = "XLSX"
        }).ToList();

        return entities;
    }

    public async Task<List<CategorySelectItem>> GetAllCategorySelectItemsAsync()
    {
        var categories = await _categoryService.GetAllAsync();
        var selectItems = new List<CategorySelectItem>();
        var defaultCategory = await _categoryService.GetDefaultCategoryAsync();
        foreach (var c in categories)
        {
            selectItems.Add(c.ToSelectItem());
        }
        selectItems.Add(defaultCategory.ToSelectItem());
        return selectItems;
    }
}