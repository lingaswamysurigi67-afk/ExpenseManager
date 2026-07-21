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
public class FeeTypesController : ControllerBase
{
    private readonly AppDbContext _db;
    public FeeTypesController(AppDbContext db) => _db = db;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? subCategoryId)
    {
        var query = _db.FeeTypes.AsQueryable();
        if (subCategoryId.HasValue) query = query.Where(f => f.SubCategoryId == subCategoryId.Value);

        var items = await query
            .OrderBy(f => f.SubCategoryId)
            .ThenBy(f => f.Name)
            .ToListAsync();

        return Ok(items.Select(FeeTypeResponse.From).ToList());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromQuery] int subCategoryId, FeeTypeRequest request)
    {
        var subCategory = await _db.SubCategories.FirstOrDefaultAsync(s => s.Id == subCategoryId);
        if (subCategory is null)
            return BadRequest(new { message = "Invalid sub-category." });

        var name = request.Name.Trim();

        var exists = await _db.FeeTypes.AnyAsync(f => f.SubCategoryId == subCategoryId && f.Name == name);
        if (exists)
            return Conflict(new { message = "A fee type with that name already exists in this sub-category." });

        var feeType = new FeeType
        {
            SubCategoryId = subCategoryId,
            Name = name,
            CreatedBy = UserId,
            CreatedDate = DateTime.UtcNow
        };

        _db.FeeTypes.Add(feeType);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = feeType.Id }, FeeTypeResponse.From(feeType));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, FeeTypeRequest request)
    {
        var feeType = await _db.FeeTypes.FirstOrDefaultAsync(f => f.Id == id);
        if (feeType is null)
            return NotFound(new { message = "Fee type not found." });

        var name = request.Name.Trim();

        var dup = await _db.FeeTypes.AnyAsync(f => f.Id != id && f.SubCategoryId == feeType.SubCategoryId && f.Name == name);
        if (dup)
            return Conflict(new { message = "A fee type with that name already exists in this sub-category." });

        feeType.Name = name;
        feeType.UpdatedBy = UserId;
        feeType.UpdatedDate = DateTime.UtcNow;

        // Keep the denormalized fee type name in sync on referencing expenses.
        var exps = await _db.Expenses.Where(e => e.FeeTypeId == id).ToListAsync();
        foreach (var e in exps) e.FeeType = name;

        await _db.SaveChangesAsync();
        return Ok(FeeTypeResponse.From(feeType));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var feeType = await _db.FeeTypes.FirstOrDefaultAsync(f => f.Id == id);
        if (feeType is null)
            return NotFound(new { message = "Fee type not found." });

        feeType.IsActive = false;
        feeType.UpdatedBy = UserId;
        feeType.UpdatedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
