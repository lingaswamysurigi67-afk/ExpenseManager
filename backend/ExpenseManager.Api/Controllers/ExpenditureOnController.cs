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
public class ExpenditureOnController : ControllerBase
{
    private readonly AppDbContext _db;
    public ExpenditureOnController(AppDbContext db) => _db = db;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.ExpenditureOns
            .Where(p => p.UserId == UserId)
            .OrderBy(p => p.Name)
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ExpenditureOnRequest request)
    {
        var name = request.Name.Trim();

        var exists = await _db.ExpenditureOns.AnyAsync(p => p.UserId == UserId && p.Name == name);
        if (exists)
            return Conflict(new { message = "That person already exists." });

        var person = new ExpenditureOn
        {
            UserId = UserId,
            Name = name,
            CreatedBy = UserId,
            CreatedDate = DateTime.UtcNow
        };

        _db.ExpenditureOns.Add(person);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = person.Id }, person);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var person = await _db.ExpenditureOns
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == UserId);
        if (person is null)
            return NotFound(new { message = "Person not found." });

        _db.ExpenditureOns.Remove(person);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
