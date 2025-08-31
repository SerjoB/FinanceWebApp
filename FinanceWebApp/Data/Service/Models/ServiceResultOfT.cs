namespace FinanceWebApp.Data.Service.Models;

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }

    public static ServiceResult<T> Ok(T data) =>
        new ServiceResult<T> { Success = true, Data = data };

    public new static ServiceResult<T> Fail(string errorMessage) =>
        new ServiceResult<T> { Success = false, ErrorMessage = errorMessage };
}