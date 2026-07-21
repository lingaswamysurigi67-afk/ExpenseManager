using System.ComponentModel.DataAnnotations;
using ExpenseManager.Api.Models;

namespace ExpenseManager.Api.Dtos;

public class ExpenseRequest
{
    [Range(0.01, 100000000)]
    public decimal Amount { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    public int? SubCategoryId { get; set; }

    public int? FeeTypeId { get; set; }

    [Range(1, int.MaxValue)]
    public int PersonId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [MaxLength(30)]
    public string PaymentMethod { get; set; } = "Cash";

    [MaxLength(300)]
    public string Notes { get; set; } = string.Empty;
}

public class ExpenseResponse
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int CategoryId { get; set; }
    public string Category { get; set; } = string.Empty;
    public int? SubCategoryId { get; set; }
    public string? SubCategory { get; set; }
    public int? FeeTypeId { get; set; }
    public string? FeeType { get; set; }
    public int? PersonId { get; set; }
    public DateTime Date { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public static ExpenseResponse From(Expense e) => new()
    {
        Id = e.Id,
        UserId = e.UserId,
        Amount = e.Amount,
        CategoryId = e.CategoryId,
        Category = e.Category,
        SubCategoryId = e.SubCategoryId,
        SubCategory = e.SubCategory,
        FeeTypeId = e.FeeTypeId,
        FeeType = e.FeeType,
        PersonId = e.PersonId,
        Date = e.Date,
        PaymentMethod = e.PaymentMethod,
        Notes = e.Notes,
        CreatedBy = e.CreatedBy,
        CreatedDate = e.CreatedDate,
        UpdatedBy = e.UpdatedBy,
        UpdatedDate = e.UpdatedDate
    };
}

public class CategoryRequest
{
    [Required, MinLength(1), MaxLength(40)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Color { get; set; } = "#4f46e5";
}

public class SubCategoryRequest
{
    [Required, MinLength(1), MaxLength(40)]
    public string Name { get; set; } = string.Empty;
}

public class SubCategoryResponse
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public static SubCategoryResponse From(SubCategory s) => new()
    {
        Id = s.Id,
        CategoryId = s.CategoryId,
        Name = s.Name,
        CreatedBy = s.CreatedBy,
        CreatedDate = s.CreatedDate,
        UpdatedBy = s.UpdatedBy,
        UpdatedDate = s.UpdatedDate
    };
}

public class FeeTypeRequest
{
    [Required, MinLength(1), MaxLength(40)]
    public string Name { get; set; } = string.Empty;
}

public class FeeTypeResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public static FeeTypeResponse From(FeeTypeCatalog f) => new()
    {
        Id = f.Id,
        Name = f.Name,
        CreatedBy = f.CreatedBy,
        CreatedDate = f.CreatedDate,
        UpdatedBy = f.UpdatedBy,
        UpdatedDate = f.UpdatedDate
    };
}

// One assignment row: a catalog fee type linked to a sub-category (name denormalized for display).
public class SubCategoryFeeTypeResponse
{
    public int Id { get; set; }
    public int SubCategoryId { get; set; }
    public int FeeTypeCatalogId { get; set; }
    public string FeeType { get; set; } = string.Empty;
}

// Replace the full set of catalog fee types assigned to a sub-category.
public class SubCategoryFeeTypeAssignRequest
{
    public List<int> FeeTypeCatalogIds { get; set; } = new();
}
