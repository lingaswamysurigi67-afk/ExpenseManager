using ExpenseManager.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<SubCategory> SubCategories => Set<SubCategory>();
    public DbSet<FeeTypeCatalog> FeeTypeCatalog => Set<FeeTypeCatalog>();
    public DbSet<SubCategoryFeeType> SubCategoryFeeTypes => Set<SubCategoryFeeType>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Income> Incomes => Set<Income>();
    public DbSet<Person> People => Set<Person>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("USERS", "Identity");
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.UserName).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Id).HasColumnName("ID");
            e.Property(u => u.UserName).HasColumnName("USER_NAME").HasMaxLength(80).IsRequired();
            e.Property(u => u.Email).HasColumnName("EMAIL").HasMaxLength(160).IsRequired();
            e.Property(u => u.PasswordHash).HasColumnName("PASSWORD_HASH").IsRequired();
            e.Property(u => u.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(100).IsRequired();
            e.Property(u => u.CreatedDate).HasColumnName("CREATED_DATE");
            e.Property(u => u.UpdatedBy).HasColumnName("UPDATED_BY").HasMaxLength(100);
            e.Property(u => u.UpdatedDate).HasColumnName("UPDATED_DATE");
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("CATEGORIES", "Config");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasColumnName("ID");
            e.Property(c => c.Name).HasColumnName("NAME").HasMaxLength(40).IsRequired();
            e.Property(c => c.Color).HasColumnName("COLOR").HasMaxLength(20);
            e.Property(c => c.IsDefault).HasColumnName("IS_DEFAULT");
            e.Property(c => c.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(100).IsRequired();
            e.Property(c => c.CreatedDate).HasColumnName("CREATED_DATE");
            e.Property(c => c.UpdatedBy).HasColumnName("UPDATED_BY").HasMaxLength(100);
            e.Property(c => c.UpdatedDate).HasColumnName("UPDATED_DATE");
            e.Property(c => c.IsActive).HasColumnName("IS_ACTIVE").HasDefaultValue(true);
            e.HasQueryFilter(c => c.IsActive);
        });

        modelBuilder.Entity<SubCategory>(e =>
        {
            e.ToTable("SUB_CATEGORIES", "Config");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasColumnName("ID");
            e.Property(s => s.CategoryId).HasColumnName("CATEGORY_ID");
            e.Property(s => s.Name).HasColumnName("NAME").HasMaxLength(40).IsRequired();
            e.Property(s => s.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(100).IsRequired();
            e.Property(s => s.CreatedDate).HasColumnName("CREATED_DATE");
            e.Property(s => s.UpdatedBy).HasColumnName("UPDATED_BY").HasMaxLength(100);
            e.Property(s => s.UpdatedDate).HasColumnName("UPDATED_DATE");
            e.Property(s => s.IsActive).HasColumnName("IS_ACTIVE").HasDefaultValue(true);
            e.HasQueryFilter(s => s.IsActive);
            e.HasIndex(s => s.CategoryId);
        });

        modelBuilder.Entity<FeeTypeCatalog>(e =>
        {
            e.ToTable("FEE_TYPE_CATALOG", "Config");
            e.HasKey(f => f.Id);
            e.Property(f => f.Id).HasColumnName("ID");
            e.Property(f => f.Name).HasColumnName("NAME").HasMaxLength(40).IsRequired();
            e.Property(f => f.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(100).IsRequired();
            e.Property(f => f.CreatedDate).HasColumnName("CREATED_DATE");
            e.Property(f => f.UpdatedBy).HasColumnName("UPDATED_BY").HasMaxLength(100);
            e.Property(f => f.UpdatedDate).HasColumnName("UPDATED_DATE");
            e.Property(f => f.IsActive).HasColumnName("IS_ACTIVE").HasDefaultValue(true);
            e.HasQueryFilter(f => f.IsActive);
        });

        modelBuilder.Entity<SubCategoryFeeType>(e =>
        {
            e.ToTable("SUB_CATEGORY_FEE_TYPES", "Config");
            e.HasKey(m => m.Id);
            e.Property(m => m.Id).HasColumnName("ID");
            e.Property(m => m.SubCategoryId).HasColumnName("SUB_CATEGORY_ID");
            e.Property(m => m.FeeTypeCatalogId).HasColumnName("FEE_TYPE_CATALOG_ID");
            e.Property(m => m.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(100).IsRequired();
            e.Property(m => m.CreatedDate).HasColumnName("CREATED_DATE");
            e.Property(m => m.UpdatedBy).HasColumnName("UPDATED_BY").HasMaxLength(100);
            e.Property(m => m.UpdatedDate).HasColumnName("UPDATED_DATE");
            e.Property(m => m.IsActive).HasColumnName("IS_ACTIVE").HasDefaultValue(true);
            e.HasQueryFilter(m => m.IsActive);
            e.HasIndex(m => m.SubCategoryId);
            e.HasIndex(m => m.FeeTypeCatalogId);
        });

        modelBuilder.Entity<Expense>(e =>
        {
            e.ToTable("EXPENSES", "People");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID");
            e.Property(x => x.UserId).HasColumnName("USER_ID");
            e.Property(x => x.Amount).HasColumnName("AMOUNT").HasPrecision(18, 2);
            e.Property(x => x.CategoryId).HasColumnName("CATEGORY_ID");
            e.Property(x => x.Category).HasColumnName("CATEGORY").HasMaxLength(40);
            e.Property(x => x.SubCategoryId).HasColumnName("SUB_CATEGORY_ID");
            e.Property(x => x.SubCategory).HasColumnName("SUB_CATEGORY").HasMaxLength(40);
            e.Property(x => x.FeeTypeId).HasColumnName("FEE_TYPE_ID");
            e.Property(x => x.FeeType).HasColumnName("FEE_TYPE").HasMaxLength(40);
            e.Property(x => x.PersonId).HasColumnName("PERSON_ID");
            e.Property(x => x.Date).HasColumnName("EXPENDITURE_DATE");
            e.Property(x => x.PaymentMethod).HasColumnName("PAYMENT_METHOD").HasMaxLength(30);
            e.Property(x => x.Notes).HasColumnName("NOTES").HasMaxLength(300);
            e.Property(x => x.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(100).IsRequired();
            e.Property(x => x.CreatedDate).HasColumnName("CREATED_DATE");
            e.Property(x => x.UpdatedBy).HasColumnName("UPDATED_BY").HasMaxLength(100);
            e.Property(x => x.UpdatedDate).HasColumnName("UPDATED_DATE");
            e.Property(x => x.IsActive).HasColumnName("IS_ACTIVE").HasDefaultValue(true);
            e.HasQueryFilter(x => x.IsActive);
            e.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<Income>(e =>
        {
            e.ToTable("INCOMES", "People");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID");
            e.Property(x => x.UserId).HasColumnName("USER_ID");
            e.Property(x => x.Amount).HasColumnName("AMOUNT").HasPrecision(18, 2);
            e.Property(x => x.CategoryId).HasColumnName("CATEGORY_ID");
            e.Property(x => x.Category).HasColumnName("CATEGORY").HasMaxLength(40);
            e.Property(x => x.PersonId).HasColumnName("PERSON_ID");
            e.Property(x => x.Source).HasColumnName("SOURCE").HasMaxLength(80);
            e.Property(x => x.Date).HasColumnName("MONEY_CAME_DATE");
            e.Property(x => x.PaymentMethod).HasColumnName("PAYMENT_METHOD").HasMaxLength(30);
            e.Property(x => x.Notes).HasColumnName("NOTES").HasMaxLength(300);
            e.Property(x => x.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(100).IsRequired();
            e.Property(x => x.CreatedDate).HasColumnName("CREATED_DATE");
            e.Property(x => x.UpdatedBy).HasColumnName("UPDATED_BY").HasMaxLength(100);
            e.Property(x => x.UpdatedDate).HasColumnName("UPDATED_DATE");
            e.Property(x => x.IsActive).HasColumnName("IS_ACTIVE").HasDefaultValue(true);
            e.HasQueryFilter(x => x.IsActive);
            e.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<Person>(e =>
        {
            e.ToTable("PEOPLE", "Config");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID");
            e.Property(x => x.UserId).HasColumnName("USER_ID");
            e.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(80).IsRequired();
            e.Property(x => x.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(100).IsRequired();
            e.Property(x => x.CreatedDate).HasColumnName("CREATED_DATE");
            e.Property(x => x.UpdatedBy).HasColumnName("UPDATED_BY").HasMaxLength(100);
            e.Property(x => x.UpdatedDate).HasColumnName("UPDATED_DATE");
            e.Property(x => x.IsActive).HasColumnName("IS_ACTIVE").HasDefaultValue(true);
            e.HasQueryFilter(x => x.IsActive);
            e.HasIndex(x => x.UserId);
        });
    }
}
