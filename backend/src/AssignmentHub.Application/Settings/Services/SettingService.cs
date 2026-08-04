using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Application.Settings.Dtos;
using AssignmentHub.Application.Settings.Interfaces;
using AssignmentHub.Application.Settings.Mappings;
using AssignmentHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentHub.Application.Settings.Services;

public class SettingService : ISettingService
{
    private readonly ISettingRepository _settingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SettingService(ISettingRepository settingRepository, IUnitOfWork unitOfWork)
    {
        _settingRepository = settingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<SettingDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _settingRepository.Query()
            .OrderBy(s => s.SettingKey)
            .Select(s => s.ToDto())
            .ToListAsync(cancellationToken);
    }

    public async Task<SettingDto> UpsertAsync(UpsertSettingRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _settingRepository.GetByKeyAsync(request.SettingKey, cancellationToken);

        if (existing is not null)
        {
            existing.SettingValue = request.SettingValue;
            _settingRepository.Update(existing);
        }
        else
        {
            existing = new Setting
            {
                SettingKey = request.SettingKey,
                SettingValue = request.SettingValue
            };
            await _settingRepository.AddAsync(existing, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return existing.ToDto();
    }
}
