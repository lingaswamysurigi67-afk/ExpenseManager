using System.ComponentModel.DataAnnotations;

namespace ExpenseManager.Api.Dtos;

public class ExpenditureOnRequest
{
    [Required, MinLength(1), MaxLength(80)]
    public string Name { get; set; } = string.Empty;
}
