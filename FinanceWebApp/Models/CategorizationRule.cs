using Microsoft.EntityFrameworkCore;

namespace FinanceWebApp.Models;

public class CategorizationRule: IUserOwnedEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string MatchText { get; set; } = null!;
    public int CategoryId { get; set; }

    public ApplicationUser ApplicationUser { get; set; } = null!;
    public Category Category { get; set; } = null!;
}