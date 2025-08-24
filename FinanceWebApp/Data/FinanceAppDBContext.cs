using FinanceWebApp.Models;

namespace FinanceWebApp.Data;
using Microsoft.EntityFrameworkCore;

public class FinanceAppDbContext: DbContext
{
    public  FinanceAppDbContext(DbContextOptions<FinanceAppDbContext> options): base(options)
    {
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<CategorizationRule> CategorizationRules { get; set; }
}