namespace ExpenseManager.Api.Dtos;

public class CategoryBreakdown
{
    public int CategoryId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Color { get; set; } = "#4f46e5";
    public decimal Total { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class MonthlyBreakdown
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int Count { get; set; }
}

public class SummaryResponse
{
    public decimal Total { get; set; }
    public int Count { get; set; }
    public decimal Average { get; set; }
    public decimal ThisMonthTotal { get; set; }
    public decimal TodayTotal { get; set; }
    public List<CategoryBreakdown> ByCategory { get; set; } = new();
    public List<MonthlyBreakdown> ByMonth { get; set; } = new();
}

public class ReceivableRow
{
    public int PersonId { get; set; }
    public string Person { get; set; } = string.Empty;
    public decimal Given { get; set; }
    public decimal Returned { get; set; }
    public decimal Remaining { get; set; }
}

public class ReceivablesResponse
{
    public decimal TotalGiven { get; set; }
    public decimal TotalReturned { get; set; }
    public decimal TotalRemaining { get; set; }
    public List<ReceivableRow> Rows { get; set; } = new();
}
