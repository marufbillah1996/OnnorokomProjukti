using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Application.Common.Interfaces;

public record AccessTokenResult(string Token, DateTime ExpiresAtUtc);

public interface IJwtTokenGenerator
{
    AccessTokenResult GenerateAccessToken(User user);

    /// <summary>Cryptographically random opaque value — never a JWT. Persisted hashed in RefreshTokens.</summary>
    string GenerateRefreshTokenValue();
}
