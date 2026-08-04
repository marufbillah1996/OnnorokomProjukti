using AssignmentHub.Application.Auth.Interfaces;
using AssignmentHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentHub.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    // Tracked — the calling service mutates RevokedAt on the returned entity before SaveChanges.
    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);
}
