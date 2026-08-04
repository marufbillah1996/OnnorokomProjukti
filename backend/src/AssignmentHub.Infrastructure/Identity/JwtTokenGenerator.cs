using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AssignmentHub.Infrastructure.Identity;

/// <summary>
/// Implements <see cref="IJwtTokenGenerator"/> using System.IdentityModel.Tokens.Jwt.
/// Reads signing/issuer settings from configuration keys "Jwt:Secret", "Jwt:Issuer",
/// "Jwt:Audience" and "Jwt:AccessTokenMinutes" (defaults to 15 minutes if missing/unparseable).
/// </summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;
    private readonly IDateTimeProvider _dateTimeProvider;

    public JwtTokenGenerator(IConfiguration configuration, IDateTimeProvider dateTimeProvider)
    {
        _configuration = configuration;
        _dateTimeProvider = dateTimeProvider;
    }

    public AccessTokenResult GenerateAccessToken(User user)
    {
        var secret = _configuration["Jwt:Secret"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        if (!int.TryParse(_configuration["Jwt:AccessTokenMinutes"], out var accessTokenMinutes))
        {
            accessTokenMinutes = 15;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("name", user.Name)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret ?? string.Empty));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var notBefore = _dateTimeProvider.UtcNow;
        var expires = notBefore.AddMinutes(accessTokenMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: notBefore,
            expires: expires,
            signingCredentials: signingCredentials);

        var handler = new JwtSecurityTokenHandler();
        var tokenString = handler.WriteToken(token);

        return new AccessTokenResult(tokenString, expires);
    }

    public string GenerateRefreshTokenValue()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
