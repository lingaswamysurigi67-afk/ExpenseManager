using System.Security.Claims;
using ExpenseManager.Api.Data;
using ExpenseManager.Api.Dtos;
using ExpenseManager.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Api.Controllers;

// Manages the reusable, global catalog of fee types (Tuition Fee, Transport Fee, …).
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class FeeTypesController : ControllerBase
{
    private readonly AppDbContext _db;
    public FeeTypesController(AppDbContext db) => _db = db;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.FeeTypeCatalog
            .OrderBy(f => f.Name)
            .ToListAsync();

        return Ok(items.Select(FeeTypeResponse.From).ToList());
    }

    [HttpPost]
    public async Task<IActionResult> Create(FeeTypeRequest request)
    {
        var name = request.Name.Trim();

        var exists = await _db.FeeTypeCatalog.AnyAsync(f => f.Name == name);
        if (exists)
            return Conflict(new { message = "A fee type with that name already exists." });

        var feeType = new FeeTypeCatalog
        {
            Name = name,
            CreatedBy = UserId,
            CreatedDate = DateTime.UtcNow
        };

        _db.FeeTypeCatalog.Add(feeType);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = feeType.Id }, FeeTypeResponse.From(feeType));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, FeeTypeRequest request)
    {
        var feeType = await _db.FeeTypeCatalog.FirstOrDefaultAsync(f => f.Id == id);
        if (feeType is null)
            return NotFound(new { message = "Fee type not found." });

        var name = request.Name.Trim();

        var dup = await _db.FeeTypeCatalog.AnyAsync(f => f.Id != id && f.Name == name);
        if (dup)
            return Conflict(new { message = "A fee type with that name already exists." });

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
        var feeType = await _db.FeeTypeCatalog.FirstOrDefaultAsync(f => f.Id == id);
        if (feeType is null)
            return NotFound(new { message = "Fee type not found." });

        feeType.IsActive = false;
        feeType.UpdatedBy = UserId;
        feeType.UpdatedDate = DateTime.UtcNow;

        // Remove this fee type from any sub-category assignments.
        var maps = await _db.SubCategoryFeeTypes.Where(m => m.FeeTypeCatalogId == id).ToListAsync();
        foreach (var m in maps)
        {
            m.IsActive = false;
            m.UpdatedBy = UserId;
            m.UpdatedDate = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
