using ExpenseManager.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Income> Incomes => Set<Income>();
    public DbSet<Person> People => Set<Person>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.UserName).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.UserName).HasMaxLength(80).IsRequired();
            e.Property(u => u.Email).HasMaxLength(160).IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.CreatedBy).HasMaxLength(100).IsRequired();
            e.Property(u => u.UpdatedBy).HasMaxLength(100);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).HasMaxLength(40).IsRequired();
            e.Property(c => c.Color).HasMaxLength(20);
            e.Property(c => c.CreatedBy).HasMaxLength(100).IsRequired();
            e.Property(c => c.UpdatedBy).HasMaxLength(100);
        });

        modelBuilder.Entity<Expense>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.Category).HasMaxLength(40);
            e.Property(x => x.PersonName).HasMaxLength(80);
            e.Property(x => x.PaymentMethod).HasMaxLength(30);
            e.Property(x => x.Notes).HasMaxLength(300);
            e.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
            e.Property(x => x.UpdatedBy).HasMaxLength(100);
            e.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<Income>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.Category).HasMaxLength(40);
            e.Property(x => x.PersonName).HasMaxLength(80);
            e.Property(x => x.Source).HasMaxLength(80);
            e.Property(x => x.PaymentMethod).HasMaxLength(30);
            e.Property(x => x.Notes).HasMaxLength(300);
            e.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
            e.Property(x => x.UpdatedBy).HasMaxLength(100);
            e.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<Person>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(80).IsRequired();
            e.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
            e.Property(x => x.UpdatedBy).HasMaxLength(100);
            e.HasIndex(x => x.UserId);
        });
    }
}
