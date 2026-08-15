using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SkillSwap.Application.Interfaces;
using SkillSwap.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SkillSwap.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        string key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key is not configured.");

        string issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT issuer is not configured.");

        string audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT audience is not configured.");

        int expirationMinutes =
            _configuration.GetValue<int>("Jwt:ExpirationMinutes");

        var now = DateTime.UtcNow;

        var claims = new List<Claim>
{
    new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
    new(JwtRegisteredClaimNames.Email, user.Email),
    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    new(
        JwtRegisteredClaimNames.Iat,
        new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
        ClaimValueTypes.Integer64),

    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new(ClaimTypes.Email, user.Email),
    new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim())
};

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}