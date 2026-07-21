using System.Globalization;

namespace ExpenseManager.Api.Services;

// Shared parsing rules for imported cell values so Expenses and Income behave identically.
public static class ImportValues
{
    // Day-first formats first (INR/India context), then ISO and month-first fallbacks.
    private static readonly string[] DateFormats =
    {
        "yyyy-MM-dd", "yyyy/MM/dd",
        "dd-MM-yyyy", "dd/MM/yyyy",
        "d-M-yyyy", "d/M/yyyy",
        "dd-MMM-yyyy", "dd MMM yyyy", "d MMM yyyy",
        "MM/dd/yyyy", "M/d/yyyy",
        "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd HH:mm:ss"
    };

    public static bool TryParseAmount(string text, out decimal amount)
    {
        amount = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;
        // Strip currency symbols, spaces and thousands separators.
        var cleaned = text.Replace("₹", "").Replace(",", "").Replace(" ", "").Trim();
        return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
    }

    public static bool TryParseDate(string text, out DateTime date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(text)) return false;
        var trimmed = text.Trim();
        if (DateTime.TryParseExact(trimmed, DateFormats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out date))
            return true;
        return DateTime.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }
}
