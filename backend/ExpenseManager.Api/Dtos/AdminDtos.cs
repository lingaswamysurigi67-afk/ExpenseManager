namespace ExpenseManager.Api.Dtos;

public class AdminUserRow
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

public class AdminUsersResponse
{
    public int TotalUsers { get; set; }
    public int FilteredCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<AdminUserRow> Users { get; set; } = new();
}
