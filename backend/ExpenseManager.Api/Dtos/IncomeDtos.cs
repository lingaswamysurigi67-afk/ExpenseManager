using System.ComponentModel.DataAnnotations;
using ExpenseManager.Api.Models;

namespace ExpenseManager.Api.Dtos;

public class IncomeRequest
{
    [Range(0.01, 100000000)]
    public decimal Amount { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue)]
    public int PersonId { get; set; }

    [MaxLength(80)]
    public string Source { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    [MaxLength(30)]
    public string PaymentMethod { get; set; } = "Cash";

    [MaxLength(300)]
    public string Notes { get; set; } = string.Empty;
}

public class IncomeResponse
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int CategoryId { get; set; }
    public string Category { get; set; } = string.Empty;
    public int? PersonId { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public static IncomeResponse From(Income i) => new()
    {
        Id = i.Id,
        UserId = i.UserId,
        Amount = i.Amount,
        CategoryId = i.CategoryId,
        Category = i.Category,
        PersonId = i.PersonId,
        Source = i.Source,
        Date = i.Date,
        PaymentMethod = i.PaymentMethod,
        Notes = i.Notes,
        CreatedBy = i.CreatedBy,
        CreatedDate = i.CreatedDate,
        UpdatedBy = i.UpdatedBy,
        UpdatedDate = i.UpdatedDate
    };
}
