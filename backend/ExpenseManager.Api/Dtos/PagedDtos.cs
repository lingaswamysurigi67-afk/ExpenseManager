namespace ExpenseManager.Api.Dtos;

// Generic server-side paged result. Items are the current page; TotalCount is the full filtered count.
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

// Expense/Income lists also carry the filtered total amount so the header stays accurate across pages.
public class ExpensePagedResult : PagedResult<ExpenseResponse>
{
    public decimal TotalAmount { get; set; }
}

public class IncomePagedResult : PagedResult<IncomeResponse>
{
    public decimal TotalAmount { get; set; }
}

// Shared helper for normalising paging/sort query parameters.
public static class Paging
{
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    public static (int page, int pageSize) Normalize(int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > MaxPageSize) pageSize = DefaultPageSize;
        return (page, pageSize);
    }

    public static bool Ascending(string? dir) =>
        !string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase);
}
