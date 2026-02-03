using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Data;
using MinimalApi.Dtos;
using MinimalApi.Models;
using MinimalApi.Services;

namespace MinimalApi.Controllers;

public class AuthController(
    ApplicationDbContext context,
    PasswordHasher<User> hasher,
    JwtTokenService tokenService) : BaseApiController
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return HandleError("Username and password are required.");
        }

        if (request.Password.Length < 6)
        {
            return HandleError("Password must be at least 6 characters.");
        }

        var normalizedUsername = request.Username.Trim();
        var exists = await context.Users.AnyAsync(u => u.Username == normalizedUsername);
        if (exists)
        {
            return HandleError("Username already exists.", StatusCodes.Status409Conflict);
        }

        var user = new User { Username = normalizedUsername };
        user.PasswordHash = hasher.HashPassword(user, request.Password);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userDto = new UserDto
        {
            Id = user.Id,
            Name = user.Username,
            Company = user.Company,
            IsVerified = user.IsVerified,
            AvatarUrl = user.AvatarUrl,
            Status = user.Status,
            Role = user.Role
        };

        return Created($"/api/User/{user.Id}", userDto);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user is null)
        {
            return Unauthorized();
        }

        var verification = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return Unauthorized();
        }

        var token = tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Username));
    }
}
