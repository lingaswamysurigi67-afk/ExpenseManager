using System.ComponentModel.DataAnnotations;

namespace ExpenseManager.Api.Dtos;

public class ExpenseRequest
{
    [Range(0.01, 100000000)]
    public decimal Amount { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [MaxLength(30)]
    public string PaymentMethod { get; set; } = "Cash";

    [MaxLength(300)]
    public string Notes { get; set; } = string.Empty;
}

public class CategoryRequest
{
    [Required, MinLength(1), MaxLength(40)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Color { get; set; } = "#4f46e5";
}
