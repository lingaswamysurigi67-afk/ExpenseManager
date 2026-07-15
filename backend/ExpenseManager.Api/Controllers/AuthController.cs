using ExpenseManager.Api.Data;
using ExpenseManager.Api.Dtos;
using ExpenseManager.Api.Models;
using ExpenseManager.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokens;
    private readonly PasswordHasher<User> _hasher = new();

    public AuthController(AppDbContext db, TokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var userName = request.UserName.Trim();
        var email = request.Email.Trim().ToLowerInvariant();

        var exists = await _db.Users.AnyAsync(u => u.UserName == userName || u.Email == email);
        if (exists)
            return Conflict(new { message = "Username or email already exists." });

        var user = new User { UserName = userName, Email = email };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);
        user.CreatedBy = "self";
        user.CreatedDate = DateTime.UtcNow;

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var (token, expires) = _tokens.CreateToken(user);
        return Ok(new AuthResponse
        {
            Token = token,
            UserName = user.UserName,
            Email = user.Email,
            ExpiresAt = expires
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var id = request.UserNameOrEmail.Trim();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == id || u.Email == id);

        if (user is null)
            return Unauthorized(new { message = "Invalid credentials." });

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Invalid credentials." });

        var (token, expires) = _tokens.CreateToken(user);
        return Ok(new AuthResponse
        {
            Token = token,
            UserName = user.UserName,
            Email = user.Email,
            ExpiresAt = expires
        });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var userName = request.UserName.Trim();
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userName && u.Email == email);
        if (user is null)
            return NotFound(new { message = "No account matches that username and email." });

        user.PasswordHash = _hasher.HashPassword(user, request.NewPassword);
        user.UpdatedBy = "self";
        user.UpdatedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Password has been reset. You can now sign in." });
    }
}