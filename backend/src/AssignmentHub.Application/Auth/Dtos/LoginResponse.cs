namespace AssignmentHub.Application.Auth.Dtos;

public record LoginResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    Guid UserId,
    string Name,
    string Email,
    string Role);
