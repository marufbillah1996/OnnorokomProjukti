using AssignmentHub.Application.Settings.Dtos;

namespace AssignmentHub.Application.Settings.Services;

public interface ISettingService
{
    Task<IReadOnlyList<SettingDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<SettingDto> UpsertAsync(UpsertSettingRequest request, CancellationToken cancellationToken = default);
}
