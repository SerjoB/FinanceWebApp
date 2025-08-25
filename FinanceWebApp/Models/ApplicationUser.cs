namespace FinanceWebApp.Models;
using Microsoft.AspNetCore.Identity;
public class ApplicationUser: IdentityUser<int>
{

    public ICollection<Category>? Categories { get; set; }
    public ICollection<Transaction>? Transactions { get; set; }
    public ICollection<CategorizationRule>? Rules { get; set; }
}