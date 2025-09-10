namespace FinanceWebApp.Models;

public interface IUserOwnedEntity
{
    int Id { get; set; }
    int UserId { get; set; }
}