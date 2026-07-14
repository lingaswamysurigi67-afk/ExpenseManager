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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CategoryRequest request)
    {
        var name = request.Name.Trim();

        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
            return NotFound(new { message = "Category not found." });

        var dup = await _db.Categories.AnyAsync(c => c.Id != id && c.Name == name);
        if (dup)
            return Conflict(new { message = "A category with that name already exists." });

        category.Name = name;
        category.Color = string.IsNullOrWhiteSpace(request.Color) ? category.Color : request.Color;
        category.UpdatedBy = UserId;
        category.UpdatedDate = DateTime.UtcNow;

        // Keep the denormalized category name in sync on referencing rows.
        var exps = await _db.Expenses.Where(e => e.CategoryId == id).ToListAsync();
        foreach (var e in exps) e.Category = name;
        var incs = await _db.Incomes.Where(i => i.CategoryId == id).ToListAsync();
        foreach (var i in incs) i.Category = name;

        await _db.SaveChangesAsync();
        return Ok(category);
    }
}
