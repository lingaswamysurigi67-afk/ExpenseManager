using System.Text;
using ClosedXML.Excel;

namespace ExpenseManager.Api.Services;

// Parses an uploaded CSV or XLSX file into header-keyed rows.
// Kept format-agnostic so controllers only deal with named columns.
public record ImportFileRow(int RowNumber, IReadOnlyDictionary<string, string> Values)
{
    public string Get(params string[] names)
    {
        foreach (var name in names)
            if (Values.TryGetValue(name, out var v) && !string.IsNullOrWhiteSpace(v))
                return v.Trim();
        return string.Empty;
    }
}

public static class ImportFileParser
{
    public const long MaxFileBytes = 5 * 1024 * 1024; // 5 MB
    public const int MaxRows = 5000;

    public static List<ImportFileRow> Parse(IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new InvalidOperationException("The file is empty.");
        if (file.Length > MaxFileBytes)
            throw new InvalidOperationException("File is too large (max 5 MB).");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return ext switch
        {
            ".csv" => ParseCsv(file),
            ".xlsx" => ParseXlsx(file),
            _ => throw new InvalidOperationException("Unsupported file type. Upload a .csv or .xlsx file.")
        };
    }

    private static List<ImportFileRow> ParseCsv(IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var text = reader.ReadToEnd();
        var records = SplitCsv(text);
        if (records.Count == 0)
            throw new InvalidOperationException("The file has no rows.");

        var headers = records[0].Select(h => h.Trim()).ToList();
        EnsureHeaders(headers);

        var rows = new List<ImportFileRow>();
        for (var i = 1; i < records.Count; i++)
        {
            var fields = records[i];
            // Skip completely blank lines.
            if (fields.All(string.IsNullOrWhiteSpace)) continue;

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var c = 0; c < headers.Count; c++)
                dict[headers[c]] = c < fields.Count ? fields[c] : string.Empty;

            rows.Add(new ImportFileRow(i + 1, dict)); // +1 => spreadsheet-style (header = row 1)
            if (rows.Count > MaxRows)
                throw new InvalidOperationException($"Too many rows (max {MaxRows}).");
        }
        return rows;
    }

    private static List<ImportFileRow> ParseXlsx(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheets.FirstOrDefault()
            ?? throw new InvalidOperationException("The workbook has no worksheets.");

        var used = sheet.RangeUsed();
        if (used is null)
            throw new InvalidOperationException("The worksheet is empty.");

        var allRows = used.RowsUsed().ToList();
        if (allRows.Count == 0)
            throw new InvalidOperationException("The worksheet is empty.");

        var headerCells = allRows[0].Cells(1, used.ColumnCount()).ToList();
        var headers = headerCells.Select(c => c.GetString().Trim()).ToList();
        EnsureHeaders(headers);

        var rows = new List<ImportFileRow>();
        for (var i = 1; i < allRows.Count; i++)
        {
            var row = allRows[i];
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var anyValue = false;
            for (var c = 0; c < headers.Count; c++)
            {
                var cell = row.Cell(c + 1);
                var value = CellToString(cell);
                if (!string.IsNullOrWhiteSpace(value)) anyValue = true;
                dict[headers[c]] = value;
            }
            if (!anyValue) continue;

            rows.Add(new ImportFileRow(row.RowNumber(), dict));
            if (rows.Count > MaxRows)
                throw new InvalidOperationException($"Too many rows (max {MaxRows}).");
        }
        return rows;
    }

    private static string CellToString(IXLCell cell)
    {
        if (cell.IsEmpty()) return string.Empty;
        // Preserve dates in ISO form so downstream date parsing is stable.
        if (cell.DataType == XLDataType.DateTime && cell.TryGetValue<DateTime>(out var dt))
            return dt.ToString("yyyy-MM-dd");
        return cell.GetString().Trim();
    }

    private static void EnsureHeaders(List<string> headers)
    {
        if (headers.Count == 0 || headers.All(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException("The first row must contain column headers.");
    }

    // Minimal RFC-4180-ish CSV parser: handles quoted fields, embedded commas/newlines, and "" escapes.
    private static List<List<string>> SplitCsv(string text)
    {
        var records = new List<List<string>>();
        var field = new StringBuilder();
        var record = new List<string>();
        var inQuotes = false;

        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                    else inQuotes = false;
                }
                else field.Append(ch);
            }
            else
            {
                switch (ch)
                {
                    case '"':
                        inQuotes = true;
                        break;
                    case ',':
                        record.Add(field.ToString());
                        field.Clear();
                        break;
                    case '\r':
                        break; // handled by \n
                    case '\n':
                        record.Add(field.ToString());
                        field.Clear();
                        records.Add(record);
                        record = new List<string>();
                        break;
                    default:
                        field.Append(ch);
                        break;
                }
            }
        }
        // Flush trailing field/record (file may not end with newline).
        if (field.Length > 0 || record.Count > 0)
        {
            record.Add(field.ToString());
            records.Add(record);
        }
        return records;
    }
}
