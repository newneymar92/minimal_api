using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MinimalApi.Models;

namespace MinimalApi.Services;

public class JwtTokenService(IConfiguration configuration)
{
    public string GenerateToken(User user)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var key = jwtSection.GetValue<string>("Key") ?? throw new InvalidOperationException("Jwt:Key is missing");
        var issuer = jwtSection.GetValue<string>("Issuer");
        var audience = jwtSection.GetValue<string>("Audience");
        var expiresMinutes = jwtSection.GetValue<int?>("ExpiresInMinutes") ?? 60;

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

