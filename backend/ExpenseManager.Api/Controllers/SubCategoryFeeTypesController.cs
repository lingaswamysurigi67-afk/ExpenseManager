using System.Security.Claims;
using ExpenseManager.Api.Data;
using ExpenseManager.Api.Dtos;
using ExpenseManager.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Api.Controllers;

// Manages which catalog fee types are assigned to which sub-categories.
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SubCategoryFeeTypesController : ControllerBase
{
    private readonly AppDbContext _db;
    public SubCategoryFeeTypesController(AppDbContext db) => _db = db;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? subCategoryId)
    {
        var maps = _db.SubCategoryFeeTypes.AsQueryable();
        if (subCategoryId.HasValue) maps = maps.Where(m => m.SubCategoryId == subCategoryId.Value);

        var rows = await (
            from m in maps
            join f in _db.FeeTypeCatalog on m.FeeTypeCatalogId equals f.Id
            orderby f.Name
            select new SubCategoryFeeTypeResponse
            {
                Id = m.Id,
                SubCategoryId = m.SubCategoryId,
                FeeTypeCatalogId = m.FeeTypeCatalogId,
                FeeType = f.Name
            }).ToListAsync();

        return Ok(rows);
    }

    // Replace the full set of catalog fee types assigned to a sub-category.
    [HttpPut]
    public async Task<IActionResult> Assign(
        [FromQuery] int subCategoryId, SubCategoryFeeTypeAssignRequest request)
    {
        var subCategory = await _db.SubCategories.FirstOrDefaultAsync(s => s.Id == subCategoryId);
        if (subCategory is null)
            return BadRequest(new { message = "Invalid sub-category." });

        var requestedIds = (request.FeeTypeCatalogIds ?? new List<int>()).Distinct().ToList();

        if (requestedIds.Count > 0)
        {
            var validCount = await _db.FeeTypeCatalog.CountAsync(f => requestedIds.Contains(f.Id));
            if (validCount != requestedIds.Count)
                return BadRequest(new { message = "One or more fee types are invalid." });
        }

        // Existing mappings (ignore the IsActive filter so we can reactivate soft-deleted ones).
        var existing = await _db.SubCategoryFeeTypes
            .IgnoreQueryFilters()
            .Where(m => m.SubCategoryId == subCategoryId)
            .ToListAsync();

        var now = DateTime.UtcNow;

        foreach (var m in existing)
        {
            var shouldBeActive = requestedIds.Contains(m.FeeTypeCatalogId);
            if (m.IsActive != shouldBeActive)
            {
                m.IsActive = shouldBeActive;
                m.UpdatedBy = UserId;
                m.UpdatedDate = now;
            }
        }

        var existingIds = existing.Select(m => m.FeeTypeCatalogId).ToHashSet();
        foreach (var id in requestedIds.Where(id => !existingIds.Contains(id)))
        {
            _db.SubCategoryFeeTypes.Add(new SubCategoryFeeType
            {
                SubCategoryId = subCategoryId,
                FeeTypeCatalogId = id,
                CreatedBy = UserId,
                CreatedDate = now
            });
        }

        await _db.SaveChangesAsync();

        var rows = await (
            from m in _db.SubCategoryFeeTypes.Where(m => m.SubCategoryId == subCategoryId)
            join f in _db.FeeTypeCatalog on m.FeeTypeCatalogId equals f.Id
            orderby f.Name
            select new SubCategoryFeeTypeResponse
            {
                Id = m.Id,
                SubCategoryId = m.SubCategoryId,
                FeeTypeCatalogId = m.FeeTypeCatalogId,
                FeeType = f.Name
            }).ToListAsync();

        return Ok(rows);
    }
}
