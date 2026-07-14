using System.ComponentModel.DataAnnotations;

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
