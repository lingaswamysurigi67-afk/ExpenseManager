using System.Security.Claims;
using ExpenseManager.Api.Data;
using ExpenseManager.Api.Dtos;
using ExpenseManager.Api.Models;
using ExpenseManager.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class IncomesController : ControllerBase
{
    private readonly AppDbContext _db;
    public IncomesController(AppDbContext db) => _db = db;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? year, [FromQuery] int? month, [FromQuery] int? categoryId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = _db.Incomes.Where(e => e.UserId == UserId);

        if (year.HasValue) query = query.Where(e => e.Date.Year == year.Value);
        if (month.HasValue) query = query.Where(e => e.Date.Month == month.Value);
        if (categoryId.HasValue) query = query.Where(e => e.CategoryId == categoryId.Value);
        if (from.HasValue) query = query.Where(e => e.Date >= from.Value.Date);
        if (to.HasValue) query = query.Where(e => e.Date <= to.Value.Date);

        var list = await query
            .OrderByDescending(e => e.Date).ThenByDescending(e => e.CreatedDate)
            .ToListAsync();
        return Ok(list.Select(IncomeResponse.From));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOne(int id)
    {
        var income = await _db.Incomes.FirstOrDefaultAsync(e => e.Id == id && e.UserId == UserId);
        return income is null ? NotFound() : Ok(IncomeResponse.From(income));
    }

    [HttpPost]
    public async Task<IActionResult> Create(IncomeRequest request)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId);
        if (category is null)
            return BadRequest(new { message = "Invalid category." });

        var person = await _db.People.FirstOrDefaultAsync(p => p.Id == request.PersonId && p.UserId == UserId);
        if (person is null)
            return BadRequest(new { message = "Invalid person." });

        var income = new Income
        {
            UserId = UserId,
            Amount = request.Amount,
            CategoryId = category.Id,
            Category = category.Name,
            PersonId = person.Id,
            Source = request.Source.Trim(),
            Date = request.Date,
            PaymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "Cash" : request.PaymentMethod,
            Notes = request.Notes?.Trim() ?? string.Empty,
            CreatedBy = UserId,
            CreatedDate = DateTime.UtcNow
        };

        _db.Incomes.Add(income);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOne), new { id = income.Id }, IncomeResponse.From(income));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, IncomeRequest request)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId);
        if (category is null)
            return BadRequest(new { message = "Invalid category." });

        var person = await _db.People.FirstOrDefaultAsync(p => p.Id == request.PersonId && p.UserId == UserId);
        if (person is null)
            return BadRequest(new { message = "Invalid person." });

        var income = await _db.Incomes.FirstOrDefaultAsync(e => e.Id == id && e.UserId == UserId);
        if (income is null) return NotFound();

        income.Amount = request.Amount;
        income.CategoryId = category.Id;
        income.Category = category.Name;
        income.PersonId = person.Id;
        income.Source = request.Source.Trim();
        income.Date = request.Date;
        income.PaymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "Cash" : request.PaymentMethod;
        income.Notes = request.Notes?.Trim() ?? string.Empty;
        income.UpdatedBy = UserId;
        income.UpdatedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(IncomeResponse.From(income));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var income = await _db.Incomes.FirstOrDefaultAsync(e => e.Id == id && e.UserId == UserId);
        if (income is null) return NotFound();

        income.IsActive = false;
        income.UpdatedBy = UserId;
        income.UpdatedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("bulk-delete")]
    public async Task<IActionResult> BulkDelete(BulkDeleteRequest request)
    {
        if (request.Ids is null || request.Ids.Count == 0)
            return BadRequest(new { message = "No ids provided." });

        var toDelete = await _db.Incomes
            .Where(e => e.UserId == UserId && request.Ids.Contains(e.Id))
            .ToListAsync();

        foreach (var income in toDelete)
        {
            income.IsActive = false;
            income.UpdatedBy = UserId;
            income.UpdatedDate = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return Ok(new { deleted = toDelete.Count });
    }

    // Parse an uploaded CSV/XLSX and return a validated preview WITHOUT writing anything.
    [HttpPost("import/preview")]
    [RequestSizeLimit(ImportFileParser.MaxFileBytes)]
    public async Task<IActionResult> ImportPreview(IFormFile file)
    {
        List<ImportFileRow> rows;
        try { rows = ImportFileParser.Parse(file); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }

        var categories = await _db.Categories
            .ToDictionaryAsync(c => c.Name, StringComparer.OrdinalIgnoreCase);
        var people = await _db.People.Where(p => p.UserId == UserId)
            .ToDictionaryAsync(p => p.Name, StringComparer.OrdinalIgnoreCase);

        var previews = rows.Select(r => BuildPreview(r, categories, people)).ToList();
        return Ok(new ImportPreviewResponse
        {
            Total = previews.Count,
            ValidCount = previews.Count(p => p.Valid),
            InvalidCount = previews.Count(p => !p.Valid),
            Rows = previews
        });
    }

    // Commit the rows the user chose to import (re-validated server-side).
    [HttpPost("import")]
    public async Task<IActionResult> Import(ImportCommitRequest request)
    {
        if (request.Rows is null || request.Rows.Count == 0)
            return BadRequest(new { message = "No rows to import." });
        if (request.Rows.Count > ImportFileParser.MaxRows)
            return BadRequest(new { message = $"Too many rows (max {ImportFileParser.MaxRows})." });

        var categories = await _db.Categories
            .ToDictionaryAsync(c => c.Name, StringComparer.OrdinalIgnoreCase);
        var people = await _db.People.Where(p => p.UserId == UserId)
            .ToDictionaryAsync(p => p.Name, StringComparer.OrdinalIgnoreCase);

        var errors = new List<ImportRowError>();
        var toAdd = new List<Income>();
        var now = DateTime.UtcNow;

        for (var i = 0; i < request.Rows.Count; i++)
        {
            var row = request.Rows[i];
            var rowNumber = i + 1;

            if (row.Amount <= 0)
            { errors.Add(new ImportRowError { RowNumber = rowNumber, Message = "Amount must be greater than 0." }); continue; }
            if (!categories.TryGetValue(row.Category?.Trim() ?? "", out var category))
            { errors.Add(new ImportRowError { RowNumber = rowNumber, Message = $"Unknown category \"{row.Category}\"." }); continue; }
            if (!people.TryGetValue(row.Person?.Trim() ?? "", out var person))
            { errors.Add(new ImportRowError { RowNumber = rowNumber, Message = $"Unknown person \"{row.Person}\"." }); continue; }

            toAdd.Add(new Income
            {
                UserId = UserId,
                Amount = row.Amount,
                CategoryId = category.Id,
                Category = category.Name,
                PersonId = person.Id,
                Source = row.Source?.Trim() ?? string.Empty,
                Date = row.Date,
                PaymentMethod = string.IsNullOrWhiteSpace(row.PaymentMethod) ? "Cash" : row.PaymentMethod.Trim(),
                Notes = row.Notes?.Trim() ?? string.Empty,
                CreatedBy = UserId,
                CreatedDate = now
            });
        }

        if (toAdd.Count > 0)
        {
            _db.Incomes.AddRange(toAdd);
            await _db.SaveChangesAsync();
        }

        return Ok(new ImportCommitResponse
        {
            Imported = toAdd.Count,
            Failed = errors.Count,
            Errors = errors
        });
    }

    private static ImportRowPreview BuildPreview(
        ImportFileRow row,
        Dictionary<string, Models.Category> categories,
        Dictionary<string, Models.Person> people)
    {
        var amountText = row.Get("Amount");
        var dateText = row.Get("Date");
        var categoryName = row.Get("Category");
        var personName = row.Get("Person");
        var source = row.Get("Source");
        var paymentMethod = row.Get("Payment Method", "PaymentMethod", "Method");
        var notes = row.Get("Notes");

        var preview = new ImportRowPreview
        {
            RowNumber = row.RowNumber,
            Category = categoryName,
            Person = personName,
            Source = source,
            DateText = dateText,
            PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? "Cash" : paymentMethod,
            Notes = notes
        };

        string? error = null;
        if (ImportValues.TryParseAmount(amountText, out var amount)) preview.Amount = amount;
        if (ImportValues.TryParseDate(dateText, out var date)) preview.Date = date;

        if (!preview.Amount.HasValue || preview.Amount <= 0)
            error ??= "Amount must be a number greater than 0.";
        if (!preview.Date.HasValue)
            error ??= "Date is missing or not a recognised format.";
        if (string.IsNullOrWhiteSpace(categoryName))
            error ??= "Category is required.";
        else if (!categories.ContainsKey(categoryName))
            error ??= $"Unknown category \"{categoryName}\".";
        if (string.IsNullOrWhiteSpace(personName))
            error ??= "Person is required.";
        else if (!people.ContainsKey(personName))
            error ??= $"Unknown person \"{personName}\".";

        preview.Valid = error is null;
        preview.Error = error;
        return preview;
    }
}
