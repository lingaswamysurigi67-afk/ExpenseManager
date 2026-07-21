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
public class SubCategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    public SubCategoriesController(AppDbContext db) => _db = db;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? categoryId)
    {
        var query = _db.SubCategories.AsQueryable();
        if (categoryId.HasValue) query = query.Where(s => s.CategoryId == categoryId.Value);

        var items = await query
            .OrderBy(s => s.CategoryId)
            .ThenBy(s => s.Name)
            .ToListAsync();

        return Ok(items.Select(SubCategoryResponse.From).ToList());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromQuery] int categoryId, SubCategoryRequest request)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
        if (category is null)
            return BadRequest(new { message = "Invalid category." });

        var name = request.Name.Trim();

        var exists = await _db.SubCategories.AnyAsync(s => s.CategoryId == categoryId && s.Name == name);
        if (exists)
            return Conflict(new { message = "A sub-category with that name already exists in this category." });

        var sub = new SubCategory
        {
            CategoryId = categoryId,
            Name = name,
            CreatedBy = UserId,
            CreatedDate = DateTime.UtcNow
        };

        _db.SubCategories.Add(sub);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = sub.Id }, SubCategoryResponse.From(sub));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SubCategoryRequest request)
    {
        var sub = await _db.SubCategories.FirstOrDefaultAsync(s => s.Id == id);
        if (sub is null)
            return NotFound(new { message = "Sub-category not found." });

        var name = request.Name.Trim();

        var dup = await _db.SubCategories.AnyAsync(s => s.Id != id && s.CategoryId == sub.CategoryId && s.Name == name);
        if (dup)
            return Conflict(new { message = "A sub-category with that name already exists in this category." });

        sub.Name = name;
        sub.UpdatedBy = UserId;
        sub.UpdatedDate = DateTime.UtcNow;

        // Keep the denormalized sub-category name in sync on referencing expenses.
        var exps = await _db.Expenses.Where(e => e.SubCategoryId == id).ToListAsync();
        foreach (var e in exps) e.SubCategory = name;

        await _db.SaveChangesAsync();
        return Ok(SubCategoryResponse.From(sub));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var sub = await _db.SubCategories.FirstOrDefaultAsync(s => s.Id == id);
        if (sub is null)
            return NotFound(new { message = "Sub-category not found." });

        sub.IsActive = false;
        sub.UpdatedBy = UserId;
        sub.UpdatedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
