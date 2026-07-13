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
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    public CategoriesController(AppDbContext db) => _db = db;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var visible = await _db.Categories
            .OrderByDescending(c => c.IsDefault)
            .ThenBy(c => c.Name)
            .ToListAsync();
        return Ok(visible);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CategoryRequest request)
    {
        var name = request.Name.Trim();

        var exists = await _db.Categories.AnyAsync(c => c.Name == name);
        if (exists)
            return Conflict(new { message = "A category with that name already exists." });

        var category = new Category
        {
            Name = name,
            Color = string.IsNullOrWhiteSpace(request.Color) ? "#4f46e5" : request.Color,
            IsDefault = false,
            CreatedBy = UserId,
            CreatedDate = DateTime.UtcNow
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = category.Id }, category);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDefault);
        if (category is null)
            return NotFound(new { message = "Category not found or cannot be deleted." });

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
