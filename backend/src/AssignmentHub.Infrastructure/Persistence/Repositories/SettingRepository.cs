using AssignmentHub.Application.Settings.Interfaces;
using AssignmentHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentHub.Infrastructure.Persistence.Repositories;

public class SettingRepository : Repository<Setting>, ISettingRepository
{
    public SettingRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    // Tracked — the calling service mutates SettingValue on the returned entity before SaveChanges.
    public Task<Setting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(s => s.SettingKey == key, cancellationToken);
}
