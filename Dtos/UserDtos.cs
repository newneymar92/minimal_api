using System.Text.Json.Serialization;

namespace MinimalApi.Dtos;

public class UserDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("company")]
    public string? Company { get; set; }
    
    [JsonPropertyName("isVerified")]
    public bool IsVerified { get; set; }
    
    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; set; }
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = "active";
    
    [JsonPropertyName("role")]
    public string Role { get; set; } = "user";
}

public class UpdateUserRequest
{
    public string? Name { get; set; }
    public string? Company { get; set; }
    public bool? IsVerified { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Status { get; set; }
    public string? Role { get; set; }
}
