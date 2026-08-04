using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Application.Settings.Interfaces;

public interface ISettingRepository : IRepository<Setting>
{
    Task<Setting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
}
