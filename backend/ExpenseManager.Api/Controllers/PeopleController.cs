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
public class PeopleController : ControllerBase
{
    private readonly AppDbContext _db;
    public PeopleController(AppDbContext db) => _db = db;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.People
            .Where(p => p.UserId == UserId)
            .OrderBy(p => p.Name)
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create(PersonRequest request)
    {
        var name = request.Name.Trim();

        var exists = await _db.People.AnyAsync(p => p.UserId == UserId && p.Name == name);
        if (exists)
            return Conflict(new { message = "That person already exists." });

        var person = new Person
        {
            UserId = UserId,
            Name = name,
            CreatedBy = UserId,
            CreatedDate = DateTime.UtcNow
        };

        _db.People.Add(person);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = person.Id }, person);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, PersonRequest request)
    {
        var name = request.Name.Trim();

        var person = await _db.People.FirstOrDefaultAsync(p => p.Id == id && p.UserId == UserId);
        if (person is null)
            return NotFound(new { message = "Person not found." });

        var dup = await _db.People.AnyAsync(p => p.UserId == UserId && p.Id != id && p.Name == name);
        if (dup)
            return Conflict(new { message = "That person already exists." });

        person.Name = name;
        person.UpdatedBy = UserId;
        person.UpdatedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(person);
    }
}
