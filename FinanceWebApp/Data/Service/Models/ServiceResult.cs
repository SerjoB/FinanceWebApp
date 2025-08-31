namespace FinanceWebApp.Data.Service.Models;

public class ServiceResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public static ServiceResult Ok() =>
        new ServiceResult { Success = true };

    public static ServiceResult Fail(string errorMessage) =>
        new ServiceResult { Success = false, ErrorMessage = errorMessage };
}