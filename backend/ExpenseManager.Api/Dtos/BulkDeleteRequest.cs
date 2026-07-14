namespace ExpenseManager.Api.Dtos;

public class BulkDeleteRequest
{
    public List<int> Ids { get; set; } = new();
}
