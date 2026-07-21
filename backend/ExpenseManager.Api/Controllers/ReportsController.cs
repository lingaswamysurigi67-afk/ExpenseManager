using System.Globalization;
using System.Security.Claims;
using ExpenseManager.Api.Data;
using ExpenseManager.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ReportsController(AppDbContext db) => _db = db;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("summary")]
    public async Task<IActionResult> Summary([FromQuery] int? year, [FromQuery] int? month)
    {
        var query = _db.Expenses.Where(e => e.UserId == UserId);
        if (year.HasValue) query = query.Where(e => e.Date.Year == year.Value);
        if (month.HasValue) query = query.Where(e => e.Date.Month == month.Value);

        var list = await query.ToListAsync();
        var categories = await _db.Categories.ToListAsync();
        var now = DateTime.UtcNow;

        var byCategory = list
            .GroupBy(e => new { e.CategoryId, e.Category })
            .Select(g => new CategoryBreakdown
            {
                CategoryId = g.Key.CategoryId,
                Category = g.Key.Category,
                Color = categories.FirstOrDefault(c => c.Id == g.Key.CategoryId)?.Color ?? "#64748b",
                Total = g.Sum(x => x.Amount),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        var grandTotal = list.Sum(e => e.Amount);
        foreach (var c in byCategory)
            c.Percentage = grandTotal == 0 ? 0 : Math.Round((double)(c.Total / grandTotal) * 100, 1);

        var byMonth = list
            .GroupBy(e => new { e.Date.Year, e.Date.Month })
            .Select(g => new MonthlyBreakdown
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Label = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(g.Key.Month)} {g.Key.Year}",
                Total = g.Sum(x => x.Amount),
                Count = g.Count()
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToList();

        var summary = new SummaryResponse
        {
            Total = grandTotal,
            Count = list.Count,
            Average = list.Count == 0 ? 0 : Math.Round(grandTotal / list.Count, 2),
            ThisMonthTotal = list.Where(e => e.Date.Year == now.Year && e.Date.Month == now.Month).Sum(e => e.Amount),
            TodayTotal = list.Where(e => e.Date.Date == now.Date).Sum(e => e.Amount),
            ByCategory = byCategory,
            ByMonth = byMonth
        };

        return Ok(summary);
    }

    [HttpGet("receivables")]
    public async Task<IActionResult> Receivables(
        [FromQuery] int? year, [FromQuery] int? month,
        [FromQuery] string? search, [FromQuery] string? sort, [FromQuery] string? dir,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        (page, pageSize) = Paging.Normalize(page, pageSize);

        var expenseQuery = _db.Expenses.Where(e => e.UserId == UserId && e.PersonId != null);
        var incomeQuery = _db.Incomes.Where(i => i.UserId == UserId && i.PersonId != null);

        if (year.HasValue)
        {
            expenseQuery = expenseQuery.Where(e => e.Date.Year == year.Value);
            incomeQuery = incomeQuery.Where(i => i.Date.Year == year.Value);
        }
        if (month.HasValue)
        {
            expenseQuery = expenseQuery.Where(e => e.Date.Month == month.Value);
            incomeQuery = incomeQuery.Where(i => i.Date.Month == month.Value);
        }

        var given = await expenseQuery
            .GroupBy(e => e.PersonId!.Value)
            .Select(g => new { PersonId = g.Key, Total = g.Sum(x => x.Amount) })
            .ToListAsync();

        var returned = await incomeQuery
            .GroupBy(i => i.PersonId!.Value)
            .Select(g => new { PersonId = g.Key, Total = g.Sum(x => x.Amount) })
            .ToListAsync();

        var givenMap = given.ToDictionary(x => x.PersonId, x => x.Total);
        var returnedMap = returned.ToDictionary(x => x.PersonId, x => x.Total);

        var personIds = givenMap.Keys.Union(returnedMap.Keys).ToList();

        var people = await _db.People
            .Where(p => p.UserId == UserId && personIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name);

        var allRows = personIds
            .Select(id =>
            {
                var g = givenMap.TryGetValue(id, out var gv) ? gv : 0m;
                var r = returnedMap.TryGetValue(id, out var rv) ? rv : 0m;
                return new ReceivableRow
                {
                    PersonId = id,
                    Person = people.TryGetValue(id, out var name) ? name : "Unknown",
                    Given = g,
                    Returned = r,
                    Remaining = g - r
                };
            })
            .Where(row => row.Remaining != 0)
            .ToList();

        // Overall totals (independent of search) power the stat cards.
        var totalGiven = allRows.Sum(r => r.Given);
        var totalReturned = allRows.Sum(r => r.Returned);
        var totalRemaining = allRows.Sum(r => r.Remaining);

        // Apply search (by person name), then sort and page in memory (one row per person).
        var filtered = string.IsNullOrWhiteSpace(search)
            ? allRows
            : allRows.Where(r => r.Person.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

        var asc = Paging.Ascending(dir);
        filtered = (sort?.ToLowerInvariant()) switch
        {
            "person" => (asc ? filtered.OrderBy(r => r.Person) : filtered.OrderByDescending(r => r.Person)).ToList(),
            "given" => (asc ? filtered.OrderBy(r => r.Given) : filtered.OrderByDescending(r => r.Given)).ToList(),
            "returned" => (asc ? filtered.OrderBy(r => r.Returned) : filtered.OrderByDescending(r => r.Returned)).ToList(),
            _ => (asc ? filtered.OrderBy(r => r.Remaining) : filtered.OrderByDescending(r => r.Remaining)).ToList(),
        };

        var pagedRows = filtered
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToList();

        var response = new ReceivablesResponse
        {
            TotalGiven = totalGiven,
            TotalReturned = totalReturned,
            TotalRemaining = totalRemaining,
            FilteredCount = filtered.Count,
            FilteredRemaining = filtered.Sum(r => r.Remaining),
            Page = page,
            PageSize = pageSize,
            Rows = pagedRows
        };

        return Ok(response);
    }

    // Spending grouped by sub-category (e.g. class) for a single person, ordered
    // chronologically, with the year-over-year increase amount and percentage.
    [HttpGet("subcategory-spending")]
    public async Task<IActionResult> SubCategorySpending(
        [FromQuery] int personId, [FromQuery] int? categoryId)
    {
        var person = await _db.People.FirstOrDefaultAsync(p => p.Id == personId && p.UserId == UserId);
        if (person is null)
            return BadRequest(new { message = "Invalid person." });

        var query = _db.Expenses
            .Where(e => e.UserId == UserId && e.PersonId == personId && e.SubCategoryId != null);
        if (categoryId.HasValue)
            query = query.Where(e => e.CategoryId == categoryId.Value);

        var grouped = await query
            .GroupBy(e => new { e.SubCategoryId, e.SubCategory })
            .Select(g => new
            {
                g.Key.SubCategoryId,
                g.Key.SubCategory,
                Total = g.Sum(x => x.Amount),
                Count = g.Count(),
                FirstYear = g.Min(x => x.Date.Year),
                LastYear = g.Max(x => x.Date.Year),
                FirstDate = g.Min(x => x.Date)
            })
            .ToListAsync();

        var ordered = grouped
            .OrderBy(x => x.FirstDate)
            .ThenBy(x => x.SubCategory)
            .ToList();

        // Fee-type breakdown within each sub-category (only rows that have a fee type).
        var feeGroups = await query
            .Where(e => e.FeeTypeId != null)
            .GroupBy(e => new { e.SubCategoryId, e.FeeTypeId, e.FeeType })
            .Select(g => new
            {
                g.Key.SubCategoryId,
                g.Key.FeeTypeId,
                g.Key.FeeType,
                Total = g.Sum(x => x.Amount),
                Count = g.Count()
            })
            .ToListAsync();

        var rows = new List<SubCategorySpendingRow>();
        decimal? prev = null;
        foreach (var g in ordered)
        {
            decimal? incAmount = prev is null ? null : g.Total - prev.Value;
            double? incPct = (prev is null || prev.Value == 0)
                ? null
                : Math.Round((double)((g.Total - prev.Value) / prev.Value) * 100, 1);

            var fees = feeGroups
                .Where(f => f.SubCategoryId == g.SubCategoryId)
                .OrderByDescending(f => f.Total)
                .Select(f => new FeeTypeBreakdown
                {
                    FeeTypeId = f.FeeTypeId,
                    FeeType = f.FeeType ?? string.Empty,
                    Total = f.Total,
                    Count = f.Count
                })
                .ToList();

            rows.Add(new SubCategorySpendingRow
            {
                SubCategoryId = g.SubCategoryId,
                SubCategory = g.SubCategory ?? string.Empty,
                FirstYear = g.FirstYear,
                LastYear = g.LastYear,
                Total = g.Total,
                Count = g.Count,
                IncreaseAmount = incAmount,
                IncreasePercentage = incPct,
                Fees = fees
            });

            prev = g.Total;
        }

        return Ok(new SubCategorySpendingResponse
        {
            PersonId = person.Id,
            Person = person.Name,
            GrandTotal = rows.Sum(r => r.Total),
            Rows = rows
        });
    }
}

