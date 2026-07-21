namespace ExpenseManager.Api.Models;

// Maps a reusable fee type (FeeTypeCatalog) to a sub-category.
public class SubCategoryFeeType
{
    public int Id { get; set; }
    public int SubCategoryId { get; set; }
    public int FeeTypeCatalogId { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool IsActive { get; set; } = true;
}
