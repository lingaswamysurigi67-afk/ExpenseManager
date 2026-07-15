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
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOne(int id)
    {
        var income = await _db.Incomes.FirstOrDefaultAsync(e => e.Id == id && e.UserId == UserId);
        return income is null ? NotFound() : Ok(income);
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
            PersonName = person.Name,
            Source = request.Source.Trim(),
            Date = request.Date,
            PaymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "Cash" : request.PaymentMethod,
            Notes = request.Notes?.Trim() ?? string.Empty,
            CreatedBy = UserId,
            CreatedDate = DateTime.UtcNow
        };

        _db.Incomes.Add(income);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOne), new { id = income.Id }, income);
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
        income.PersonName = person.Name;
        income.Source = request.Source.Trim();
        income.Date = request.Date;
        income.PaymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "Cash" : request.PaymentMethod;
        income.Notes = request.Notes?.Trim() ?? string.Empty;
        income.UpdatedBy = UserId;
        income.UpdatedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(income);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var income = await _db.Incomes.FirstOrDefaultAsync(e => e.Id == id && e.UserId == UserId);
        if (income is null) return NotFound();

        _db.Incomes.Remove(income);
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

        _db.Incomes.RemoveRange(toDelete);
        await _db.SaveChangesAsync();
        return Ok(new { deleted = toDelete.Count });
    }
}
