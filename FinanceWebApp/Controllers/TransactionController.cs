using FinanceWebApp.Data.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceWebApp.Controllers;

[Authorize]
public class TransactionsController : Controller
{
    ITransactionImportService _importService;

    public TransactionsController(ITransactionImportService importService)
    {
        _importService = importService;
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
        
        var transactionModels = _importService.ImportAsync(file.OpenReadStream(), file.FileName);

        return View("Index");
    }

}
