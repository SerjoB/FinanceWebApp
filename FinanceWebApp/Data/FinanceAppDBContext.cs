using FinanceWebApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace FinanceWebApp.Data;
using Microsoft.EntityFrameworkCore;

public class FinanceAppDbContext: IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public  FinanceAppDbContext(DbContextOptions<FinanceAppDbContext> options): base(options)
    {
    }
    public DbSet<ApplicationUser> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<CategorizationRule> CategorizationRules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Category>()
            .HasOne(c => c.ApplicationUser)
            .WithMany(u => u.Categories)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        
        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.Subcategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict); // prevent cascade delete cycles
        // Category -> ParentCategory (self-reference, optional)
        modelBuilder.Entity<Category>()
            .HasData(new Category
            {
                Name = "Uncategorized",
                Type = "None",
                Id = 1,
                ParentCategoryId = 1,
                UserId = 3  //TODO: Change it to admin ID or something
            });

        // ---------------------------
        // Transaction -> ApplicationUser (many-to-1)
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.ApplicationUser)
            .WithMany(u => u.Transactions)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Transaction -> Category (optional)
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.SetNull); // TODO: we need to make it "Uncategorized" not null

        // ---------------------------
        // CategorizationRule -> ApplicationUser (many-to-1)
        modelBuilder.Entity<CategorizationRule>()
            .HasOne(r => r.ApplicationUser)
            .WithMany(u => u.Rules)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // CategorizationRule -> Category (many-to-1)
        modelBuilder.Entity<CategorizationRule>()
            .HasOne(r => r.Category)
            .WithMany()
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}