using System.Data;
using ExcelDataReader;
using FinanceWebApp.Data;
using FinanceWebApp.Data.Service;
using FinanceWebApp.Data.Service.Models;
using FinanceWebApp.Models;
using FinanceWebApp.Models.Enums;

public class TransactionImportService: ITransactionImportService
{
    private readonly FinanceAppDbContext _context;
    IRepository<Transaction>  _repository;
    IRepository<Category>  _categoryRepository;

    public TransactionImportService(FinanceAppDbContext context,
        IRepository<Transaction> repository,
        IRepository<Category> categoryRepository)
    {
        _context = context;
        _repository = repository;
        _categoryRepository = categoryRepository;
    }
    
    public async Task<Transaction?> GetTransactionAsync(QueryOptions<Transaction> optionss)
        => await _repository.GetEntityAsync(optionss);
    public async Task<List<TransactionImportModel>> ImportAsync(Stream fileStream, string fileName)
    {
        var fileType = DetectFileType(fileName);

        return fileType switch
        {
            FileType.Csv => await ParseCsvAsync(fileStream),
            FileType.Excel => await ParseExcelAsync(fileStream),
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

    private async Task<List<TransactionImportModel>> ParseCsvAsync(Stream fileStream) 
    {
        throw new NotImplementedException();
    }

    private async Task<List<TransactionImportModel>> ParseExcelAsync(Stream fileStream)
    {
        return await Task.Run(() =>
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var transactions = new List<TransactionImportModel>();

            using var reader = ExcelReaderFactory.CreateReader(fileStream);
            var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = true
                }
            });

            var table = dataSet.Tables[0]; // first worksheet

            // Detect headers
            int dateCol = -1, amountCol = -1, descCol = -1, catCol = -1;
            for (int i = 0; i < table.Columns.Count; i++)
            {
                var colName = table.Columns[i].ColumnName.ToLowerInvariant();
                if (colName.Contains("date")) dateCol = i;
                if (colName.Contains("category")) dateCol = i;
                if (colName.Contains("amount") || colName.Contains("sum")) amountCol = i;
                if (colName.Contains("desc") || colName.Contains("memo") || colName.Contains("details")) descCol = i;
            }

            foreach (DataRow row in table.Rows)
            {
                if (dateCol == -1 || amountCol == -1) continue; // can't parse without essentials

                var model = new TransactionImportModel
                {
                    Date = DateTime.TryParse(row[dateCol]?.ToString(), out var d) ? d : DateTime.MinValue,
                    Amount = decimal.TryParse(row[amountCol]?.ToString(), out var a) ? a : 0,
                    Description = descCol != -1 ? row[descCol]?.ToString() ?? "" : "",
                    CategoryName = descCol != -1 ? row[descCol]?.ToString() ?? "" : ""
                };

                // Skip empty or garbage rows
                if (model.Date == DateTime.MinValue && model.Amount == 0 &&
                    string.IsNullOrWhiteSpace(model.Description))
                    continue;

                transactions.Add(model);
            }

            return transactions;
        });
    }
    
    public async Task SaveTransactionsAsync(List<TransactionImportModel> importedTransactions)  // TODO: it's not tested yet.
                                                                                                // We need to add preview view and also relocate transaction creation before preview.
                                                                                                // This should be called by clicking "Save" on preview page
    {
        if (importedTransactions == null || importedTransactions.Count == 0)
            return;
        List<Transaction> entities = new List<Transaction>();
        for (int i = 0; i < importedTransactions.Count; i++)
        {
            var transaction = new Transaction();
            transaction.Date = importedTransactions[i].Date;
            transaction.Amount = importedTransactions[i].Amount;
            transaction.Description = importedTransactions[i].Description;
            transaction.CategoryId = await GetCategoryIdByNameAsync(importedTransactions[i].CategoryName);
            entities.Add(transaction);
        }
    
        await _context.Transactions.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
    }

    private async Task<int> GetCategoryIdByNameAsync(string categoryName)
    {
        var category = await _categoryRepository.GetEntityAsync(new QueryOptions<Category>() {Filter = c => c.Name == categoryName});
        return category == null ? 1 : category.Id;
    }

}
