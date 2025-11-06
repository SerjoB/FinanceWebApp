using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using ExcelDataReader;
using FinanceWebApp.Data.Service.Models;
using FinanceWebApp.Models;
using FinanceWebApp.Models.DTOs;
using FinanceWebApp.Models.Enums;

namespace FinanceWebApp.Data.Service;

public class TransactionService: ITransactionService
{
    IRepository<Transaction>  _repository;

    public TransactionService(IRepository<Transaction> repository)
    {
        _repository = repository;
        
    }
    
    public async Task<ServiceResult> AddRange(List<Transaction> transaction)
        =>  await _repository.AddRange(transaction);
   
    public async Task<Transaction?> GetTransactionAsync(QueryOptions<Transaction> optionss)
        => await _repository.GetEntityAsync(optionss);
    
    public async Task<ApplicationUser?> GetCurrentUserAsync() 
        => await _repository.GetCurrentUserAsync();
}