using AssignmentHub.Application.Auth.Dtos;
using AssignmentHub.Application.Auth.Interfaces;
using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Application.Common.Utilities;
using AssignmentHub.Application.Users.Interfaces;
using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Application.Auth.Services;

public class AuthService : IAuthService
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || !user.IsActive || !_passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<LoginResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = RefreshTokenHasher.Hash(request.RefreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (existingToken is null || !existingToken.IsActive(_dateTimeProvider.UtcNow))
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        var user = await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        existingToken.RevokedAt = _dateTimeProvider.UtcNow;
        _refreshTokenRepository.Update(existingToken);

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = RefreshTokenHasher.Hash(refreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (existingToken is null || !existingToken.IsActive(_dateTimeProvider.UtcNow))
        {
            return;
        }

        existingToken.RevokedAt = _dateTimeProvider.UtcNow;
        _refreshTokenRepository.Update(existingToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<LoginResponse> IssueTokensAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshTokenValue = _jwtTokenGenerator.GenerateRefreshTokenValue();
        var refreshTokenExpiresAt = _dateTimeProvider.UtcNow.Add(RefreshTokenLifetime);

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = RefreshTokenHasher.Hash(refreshTokenValue),
            ExpiresAt = refreshTokenExpiresAt,
        };

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            refreshTokenValue,
            refreshTokenExpiresAt,
            user.Id,
            user.Name,
            user.Email,
            user.Role.ToString());
    }
}
