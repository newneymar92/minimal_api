using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Data;
using MinimalApi.Dtos;
using MinimalApi.Models;

namespace MinimalApi.Controllers;

[Authorize]
public class UserController(ApplicationDbContext context) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        var users = await context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Username,
                Company = u.Company,
                IsVerified = u.IsVerified,
                AvatarUrl = u.AvatarUrl,
                Status = u.Status,
                Role = u.Role
            })
            .ToListAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

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

        return Ok(userDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
    {
        var user = await context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        // Update only provided fields
        if (request.Name != null)
        {
            var normalizedUsername = request.Name.Trim();
            // Check if username already exists (excluding current user)
            var exists = await context.Users.AnyAsync(u => u.Username == normalizedUsername && u.Id != id);
            if (exists)
            {
                return HandleError("Username already exists.", StatusCodes.Status409Conflict);
            }
            user.Username = normalizedUsername;
        }

        if (request.Company != null)
        {
            user.Company = request.Company;
        }

        if (request.IsVerified.HasValue)
        {
            user.IsVerified = request.IsVerified.Value;
        }

        if (request.AvatarUrl != null)
        {
            user.AvatarUrl = request.AvatarUrl;
        }

        if (request.Status != null)
        {
            user.Status = request.Status;
        }

        if (request.Role != null)
        {
            user.Role = request.Role;
        }

        await context.SaveChangesAsync();

        var updatedUserDto = new UserDto
        {
            Id = user.Id,
            Name = user.Username,
            Company = user.Company,
            IsVerified = user.IsVerified,
            AvatarUrl = user.AvatarUrl,
            Status = user.Status,
            Role = user.Role
        };

        return Ok(updatedUserDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        context.Users.Remove(user);
        await context.SaveChangesAsync();

        return NoContent();
    }
}
