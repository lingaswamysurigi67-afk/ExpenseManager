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
}
