using ExpenseManager.Api.Data;
using ExpenseManager.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) => _db = db;

    // Read-only list of registered users. Password hashes are never returned.
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search, [FromQuery] string? sort, [FromQuery] string? dir,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        (page, pageSize) = Paging.Normalize(page, pageSize);

        var totalUsers = await _db.Users.CountAsync();

        var query = _db.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = $"%{search.Trim()}%";
            query = query.Where(u =>
                EF.Functions.ILike(u.UserName, s) || EF.Functions.ILike(u.Email, s));
        }

        var filteredCount = await query.CountAsync();

        var asc = Paging.Ascending(dir);
        query = (sort?.ToLowerInvariant()) switch
        {
            "username" => asc ? query.OrderBy(u => u.UserName) : query.OrderByDescending(u => u.UserName),
            "email" => asc ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
            _ => asc ? query.OrderBy(u => u.CreatedDate) : query.OrderByDescending(u => u.CreatedDate),
        };

        var users = await query
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(u => new AdminUserRow
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                CreatedDate = u.CreatedDate,
                UpdatedDate = u.UpdatedDate
            })
            .ToListAsync();

        return Ok(new AdminUsersResponse
        {
            TotalUsers = totalUsers,
            FilteredCount = filteredCount,
            Page = page,
            PageSize = pageSize,
            Users = users
        });
    }
}
