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
    public async Task<IActionResult> GetUsers()
    {
        var users = await _db.Users
            .OrderByDescending(u => u.CreatedDate)
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
            TotalUsers = users.Count,
            Users = users
        });
    }
}
