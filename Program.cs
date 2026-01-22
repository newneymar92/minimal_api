using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MinimalApi.Data;
using MinimalApi.Dtos;
using MinimalApi.Models;
using MinimalApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<PasswordHasher<User>>();
builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var jwtSection = builder.Configuration.GetSection("Jwt");
    var key = jwtSection.GetValue<string>("Key") ?? throw new InvalidOperationException("Jwt:Key is missing");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtSection.GetValue<string>("Issuer"),
        ValidAudience = jwtSection.GetValue<string>("Audience"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInit");
    try
    {
        db.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        // Don't crash the app if SQL Server isn't reachable yet (common in dev).
        logger.LogWarning(ex, "Database initialization failed. Check your SQL Server connection string / instance.");
    }
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

var authGroup = app.MapGroup("/auth").WithTags("Auth");

authGroup.MapPost("/register", async (RegisterRequest request, ApplicationDbContext db, PasswordHasher<User> hasher) =>
{
    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest("Username and password are required.");
    }

    if (request.Password.Length < 6)
    {
        return Results.BadRequest("Password must be at least 6 characters.");
    }

    var normalizedUsername = request.Username.Trim();
    var exists = await db.Users.AnyAsync(u => u.Username == normalizedUsername);
    if (exists)
    {
        return Results.Conflict("Username already exists.");
    }

    var user = new User { Username = normalizedUsername };
    user.PasswordHash = hasher.HashPassword(user, request.Password);

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Created($"/users/{user.Id}", new { user.Id, user.Username, user.CreatedAt });
});

authGroup.MapPost("/login", async (LoginRequest request, ApplicationDbContext db, PasswordHasher<User> hasher, JwtTokenService tokenService) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var verification = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
    if (verification == PasswordVerificationResult.Failed)
    {
        return Results.Unauthorized();
    }

    var token = tokenService.GenerateToken(user);
    return Results.Ok(new AuthResponse(token, user.Username));
});

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.RequireAuthorization();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
