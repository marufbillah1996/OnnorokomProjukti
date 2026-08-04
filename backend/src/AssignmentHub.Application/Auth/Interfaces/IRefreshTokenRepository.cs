using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Application.Auth.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
}
