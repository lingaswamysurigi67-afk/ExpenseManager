namespace ExpenseManager.Api.Dtos;

// A single parsed row returned by the preview endpoint, annotated with validation status.
public class ImportRowPreview
{
    public int RowNumber { get; set; }
    public decimal? Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Person { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string DateText { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = "Cash";
    public string Notes { get; set; } = string.Empty;
    public bool Valid { get; set; }
    public string? Error { get; set; }
}

public class ImportPreviewResponse
{
    public int Total { get; set; }
    public int ValidCount { get; set; }
    public int InvalidCount { get; set; }
    public List<ImportRowPreview> Rows { get; set; } = new();
}

// A row the client asks us to commit (only the rows the user chose to import).
public class ImportCommitRow
{
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Person { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string Notes { get; set; } = string.Empty;
}

public class ImportCommitRequest
{
    public List<ImportCommitRow> Rows { get; set; } = new();
}

public class ImportRowError
{
    public int RowNumber { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ImportCommitResponse
{
    public int Imported { get; set; }
    public int Failed { get; set; }
    public List<ImportRowError> Errors { get; set; } = new();
}
