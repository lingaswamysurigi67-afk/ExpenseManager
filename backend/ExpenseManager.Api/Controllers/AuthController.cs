using ExpenseManager.Api.Data;
using ExpenseManager.Api.Dtos;
using ExpenseManager.Api.Models;
using ExpenseManager.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokens;
    private readonly EmailService _email;
    private readonly IConfiguration _config;
    private readonly PasswordHasher<User> _hasher = new();

    public AuthController(AppDbContext db, TokenService tokens, EmailService email, IConfiguration config)
    {
        _db = db;
        _tokens = tokens;
        _email = email;
        _config = config;
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
        user.CreatedBy = AuditUsers.Self;
        user.CreatedDate = DateTime.UtcNow;

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var (token, expires) = _tokens.CreateToken(user);
        return Ok(new AuthResponse
        {
            Token = token,
            UserName = user.UserName,
            Email = user.Email,
            IsAdmin = _tokens.IsAdmin(user.Email),
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
            IsAdmin = _tokens.IsAdmin(user.Email),
            ExpiresAt = expires
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var id = request.UserNameOrEmail.Trim();
        var idLower = id.ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == id || u.Email == idLower);

        // Always return the same response to avoid revealing whether an account exists.
        if (user is not null)
        {
            var token = _tokens.CreatePasswordResetToken(user);
            var baseUrl = (_config["App:FrontendUrl"] ?? "http://localhost:5173").TrimEnd('/');
            var link = $"{baseUrl}/reset-password?token={Uri.EscapeDataString(token)}";

            var body = $"""
                <p>Hi {user.UserName},</p>
                <p>We received a request to reset your ExpenseFlow password. This link is valid for 30 minutes:</p>
                <p><a href="{link}">Reset your password</a></p>
                <p>If you didn't request this, you can safely ignore this email.</p>
                """;

            await _email.SendAsync(user.Email, "Reset your ExpenseFlow password", body);
        }

        return Ok(new { message = "If an account matches, a password reset link has been sent to its email." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var validated = _tokens.ValidatePasswordResetToken(request.Token);
        if (validated is null)
            return BadRequest(new { message = "This reset link is invalid or has expired. Please request a new one." });

        var (userId, stamp) = validated.Value;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null || TokenService.Stamp(user.PasswordHash) != stamp)
            return BadRequest(new { message = "This reset link is invalid or has already been used. Please request a new one." });

        user.PasswordHash = _hasher.HashPassword(user, request.NewPassword);
        user.UpdatedBy = AuditUsers.Self;
        user.UpdatedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Password has been reset. You can now sign in." });
    }
}