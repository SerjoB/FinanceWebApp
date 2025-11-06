using FinanceWebApp.Data.Service;
using FinanceWebApp.Data.Service.Models;
using FinanceWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceWebApp.Controllers;

[Authorize]
public class TransactionsController : Controller
{
    readonly ITransactionService _transactionService;
    readonly ITransactionImportService _transactionImportService;

    public TransactionsController(ITransactionService transactionService,
        ITransactionImportService transactionImportService)
    {
        _transactionService = transactionService;
        _transactionImportService = transactionImportService;
    }
    
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("", "Please select a file to upload.");
            return View("Index");
        }
        
        var transactionModels = await _transactionImportService.ImportAsync(file.OpenReadStream(), file.FileName);
        var viewModel = new TransactionImportPreviewViewModel
        {
            Transactions = transactionModels,
            AllCategories = await _transactionImportService.GetAllCategorySelectItemsAsync()
        };

        return View("Preview",viewModel);
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmUpload([FromForm] TransactionImportPreviewViewModel vm)
    {
        var result = await _transactionImportService.SaveTransactionsAsync(vm.Transactions);
        if (!result.Success)
        {
            TempData["Error"] = result.ErrorMessage;
        }
        return RedirectToAction(nameof(Index));
    }

}
