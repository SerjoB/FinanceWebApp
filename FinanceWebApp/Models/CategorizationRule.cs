using Microsoft.EntityFrameworkCore;

namespace FinanceWebApp.Models;

public class CategorizationRule
{
    public int CategorizationRuleId { get; set; }
    public int UserId { get; set; }
    public string MatchText { get; set; } = null!;
    public int CategoryId { get; set; }

    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
}