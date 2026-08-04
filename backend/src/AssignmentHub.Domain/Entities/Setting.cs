using AssignmentHub.Domain.Common;

namespace AssignmentHub.Domain.Entities;

/// <summary>Generic admin-managed key/value application setting (e.g. LateSubmissionPolicy, MaintenanceMode).</summary>
public class Setting : BaseEntity
{
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
}
