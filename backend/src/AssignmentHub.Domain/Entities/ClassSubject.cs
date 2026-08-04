using AssignmentHub.Domain.Common;

namespace AssignmentHub.Domain.Entities;

/// <summary>
/// Join entity binding one Subject taught within one Class to exactly one Teacher.
/// Unique on (ClassId, SubjectId) — enforced in the EF Core configuration.
/// </summary>
public class ClassSubject : BaseEntity
{
    public Guid ClassId { get; set; }
    public Class? Class { get; set; }

    public Guid SubjectId { get; set; }
    public Subject? Subject { get; set; }

    public Guid TeacherId { get; set; }
    public User? Teacher { get; set; }

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
