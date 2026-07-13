namespace ExpenseManager.Api.Models;

public class Income
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int CategoryId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string PaymentMethod { get; set; } = "Cash";
    public string Notes { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
