namespace MinimalApi.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Company { get; set; }
    public bool IsVerified { get; set; } = false;
    public string? AvatarUrl { get; set; }
    public string Status { get; set; } = "active"; // active, banned
    public string Role { get; set; } = "user";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

