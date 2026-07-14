using System.Security.Claims;
using ExpenseManager.Api.Data;
using ExpenseManager.Api.Dtos;
using ExpenseManager.Api.Models;
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
    public async Task<IActionResult> GetAll([FromQuery] int? year, [FromQuery] int? month, [FromQuery] int? categoryId)
    {
        var query = _db.Expenses.Where(e => e.UserId == UserId);

        if (year.HasValue) query = query.Where(e => e.Date.Year == year.Value);
        if (month.HasValue) query = query.Where(e => e.Date.Month == month.Value);
        if (categoryId.HasValue) query = query.Where(e => e.CategoryId == categoryId.Value);

        var list = await query
            .OrderByDescending(e => e.Date).ThenByDescending(e => e.CreatedDate)
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOne(int id)
    {
        var expense = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == UserId);
        return expense is null ? NotFound() : Ok(expense);
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
            PersonName = person.Name,
            Date = request.Date,
            PaymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "Cash" : request.PaymentMethod,
            Notes = request.Notes?.Trim() ?? string.Empty,
            CreatedBy = UserId,
            CreatedDate = DateTime.UtcNow
        };

        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOne), new { id = expense.Id }, expense);
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
        expense.PersonName = person.Name;
        expense.Date = request.Date;
        expense.PaymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "Cash" : request.PaymentMethod;
        expense.Notes = request.Notes?.Trim() ?? string.Empty;
        expense.UpdatedBy = UserId;
        expense.UpdatedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(expense);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var expense = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == UserId);
        if (expense is null) return NotFound();

        _db.Expenses.Remove(expense);
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

        _db.Expenses.RemoveRange(toDelete);
        await _db.SaveChangesAsync();
        return Ok(new { deleted = toDelete.Count });
    }
}
