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
public class ExpensesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ExpensesController(AppDbContext db) => _db = db;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? year, [FromQuery] int? month, [FromQuery] int? categoryId,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] string? search, [FromQuery] string? sort, [FromQuery] string? dir,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        (page, pageSize) = Paging.Normalize(page, pageSize);

        // Left-join People so we can search and sort by the person's name.
        var query =
            from e in _db.Expenses.Where(e => e.UserId == UserId)
            join p in _db.People on e.PersonId equals p.Id into pj
            from p in pj.DefaultIfEmpty()
            select new { e, PersonName = (string?)(p == null ? null : p.Name) };

        if (year.HasValue) query = query.Where(x => x.e.Date.Year == year.Value);
        if (month.HasValue) query = query.Where(x => x.e.Date.Month == month.Value);
        if (categoryId.HasValue) query = query.Where(x => x.e.CategoryId == categoryId.Value);
        if (from.HasValue) query = query.Where(x => x.e.Date >= from.Value.Date);
        if (to.HasValue) query = query.Where(x => x.e.Date <= to.Value.Date);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = $"%{search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.e.Category, s) ||
                EF.Functions.ILike(x.e.Notes, s) ||
                EF.Functions.ILike(x.e.PaymentMethod, s) ||
                (x.PersonName != null && EF.Functions.ILike(x.PersonName, s)));
        }

        var totalCount = await query.CountAsync();
        var totalAmount = totalCount == 0 ? 0m : await query.SumAsync(x => x.e.Amount);

        var asc = Paging.Ascending(dir);
        query = (sort?.ToLowerInvariant()) switch
        {
            "amount" => asc ? query.OrderBy(x => x.e.Amount) : query.OrderByDescending(x => x.e.Amount),
            "person" => asc ? query.OrderBy(x => x.PersonName) : query.OrderByDescending(x => x.PersonName),
            "category" => asc ? query.OrderBy(x => x.e.Category) : query.OrderByDescending(x => x.e.Category),
            _ => asc
                ? query.OrderBy(x => x.e.Date).ThenBy(x => x.e.CreatedDate)
                : query.OrderByDescending(x => x.e.Date).ThenByDescending(x => x.e.CreatedDate),
        };

        var items = await query
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => x.e)
            .ToListAsync();

        return Ok(new ExpensePagedResult
        {
            Items = items.Select(ExpenseResponse.From).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalAmount = totalAmount
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOne(int id)
    {
        var expense = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == UserId);
        return expense is null ? NotFound() : Ok(ExpenseResponse.From(expense));
    }

    [HttpPost]
    public async Task<IActionResult> Create(ExpenseRequest request)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId);
        if (category is null)
            return BadRequest(new { message = "Invalid category." });

        var person = await _db.People.FirstOrDefaultAsync(p => p.Id == request.PersonId && p.UserId == UserId);
        if (person is null)
            return BadRequest(new { message = "Invalid person." });

        var expense = new Expense
        {
            UserId = UserId,
            Amount = request.Amount,
            CategoryId = category.Id,
            Category = category.Name,
            PersonId = person.Id,
            Date = request.Date,
            PaymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "Cash" : request.PaymentMethod,
            Notes = request.Notes?.Trim() ?? string.Empty,
            CreatedBy = UserId,
            CreatedDate = DateTime.UtcNow
        };

        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOne), new { id = expense.Id }, ExpenseResponse.From(expense));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ExpenseRequest request)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId);
        if (category is null)
            return BadRequest(new { message = "Invalid category." });

        var person = await _db.People.FirstOrDefaultAsync(p => p.Id == request.PersonId && p.UserId == UserId);
        if (person is null)
            return BadRequest(new { message = "Invalid person." });

        var expense = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == UserId);
        if (expense is null) return NotFound();

        expense.Amount = request.Amount;
        expense.CategoryId = category.Id;
        expense.Category = category.Name;
        expense.PersonId = person.Id;
        expense.Date = request.Date;
        expense.PaymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "Cash" : request.PaymentMethod;
        expense.Notes = request.Notes?.Trim() ?? string.Empty;
        expense.UpdatedBy = UserId;
        expense.UpdatedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(ExpenseResponse.From(expense));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var expense = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == UserId);
        if (expense is null) return NotFound();

        expense.IsActive = false;
        expense.UpdatedBy = UserId;
        expense.UpdatedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("bulk-delete")]
    public async Task<IActionResult> BulkDelete(BulkDeleteRequest request)
    {
        if (request.Ids is null || request.Ids.Count == 0)
            return BadRequest(new { message = "No ids provided." });

        var toDelete = await _db.Expenses
            .Where(e => e.UserId == UserId && request.Ids.Contains(e.Id))
            .ToListAsync();

        foreach (var expense in toDelete)
        {
            expense.IsActive = false;
            expense.UpdatedBy = UserId;
            expense.UpdatedDate = DateTime.UtcNow;
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
        var toAdd = new List<Expense>();
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

            toAdd.Add(new Expense
            {
                UserId = UserId,
                Amount = row.Amount,
                CategoryId = category.Id,
                Category = category.Name,
                PersonId = person.Id,
                Date = row.Date,
                PaymentMethod = string.IsNullOrWhiteSpace(row.PaymentMethod) ? "Cash" : row.PaymentMethod.Trim(),
                Notes = row.Notes?.Trim() ?? string.Empty,
                CreatedBy = UserId,
                CreatedDate = now
            });
        }

        if (toAdd.Count > 0)
        {
            _db.Expenses.AddRange(toAdd);
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
        var personName = row.Get("Person", "Expenditure On");
        var paymentMethod = row.Get("Payment Method", "PaymentMethod", "Method");
        var notes = row.Get("Notes");

        var preview = new ImportRowPreview
        {
            RowNumber = row.RowNumber,
            Category = categoryName,
            Person = personName,
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
