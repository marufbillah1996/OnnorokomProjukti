using AssignmentHub.Application.Settings.Dtos;
using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Application.Settings.Mappings;

public static class SettingMappingExtensions
{
    public static SettingDto ToDto(this Setting s) => new(s.Id, s.SettingKey, s.SettingValue);
}
