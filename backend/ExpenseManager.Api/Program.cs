using System.Text;
using ExpenseManager.Api;
using ExpenseManager.Api.Data;
using ExpenseManager.Api.Models;
using ExpenseManager.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// PostgreSQL: keep DateTime handling lenient (maps to 'timestamp without time zone').
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<TokenService>();

// CORS for the React dev server
var corsPolicy = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        var origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                      ?? new[] { "http://localhost:5173" };
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// JWT authentication
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler();

// Apply migrations (and seed default categories) on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    // Seed the built-in default categories once (IDs 1-8) if none exist yet.
    if (!await db.Categories.AnyAsync(c => c.IsDefault))
    {
        db.Categories.AddRange(
            new Category { Name = "Food & Dining",     Color = "#ef4444", IsDefault = true, CreatedBy = AuditUsers.System },
            new Category { Name = "Groceries",         Color = "#22c55e", IsDefault = true, CreatedBy = AuditUsers.System },
            new Category { Name = "Transport",         Color = "#3b82f6", IsDefault = true, CreatedBy = AuditUsers.System },
            new Category { Name = "Bills & Utilities", Color = "#f59e0b", IsDefault = true, CreatedBy = AuditUsers.System },
            new Category { Name = "Shopping",          Color = "#a855f7", IsDefault = true, CreatedBy = AuditUsers.System },
            new Category { Name = "Entertainment",     Color = "#ec4899", IsDefault = true, CreatedBy = AuditUsers.System },
            new Category { Name = "Health",            Color = "#14b8a6", IsDefault = true, CreatedBy = AuditUsers.System },
            new Category { Name = "Other",             Color = "#64748b", IsDefault = true, CreatedBy = AuditUsers.System }
        );
        await db.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(corsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
