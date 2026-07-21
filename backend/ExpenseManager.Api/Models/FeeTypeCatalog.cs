namespace ExpenseManager.Api.Models;

// Reusable, global fee type (e.g. Tuition Fee, Transport Fee, Books & Uniform)
// that can be assigned to any number of sub-categories.
public class FeeTypeCatalog
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool IsActive { get; set; } = true;
}
