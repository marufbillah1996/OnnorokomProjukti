using AssignmentHub.Domain.Common;
using AssignmentHub.Domain.Enums;

namespace AssignmentHub.Domain.Entities;

/// <summary>
/// Immutable audit trail row written every time a teacher grades or changes the status
/// of a submission — who did it, when, and what changed. Never soft-deleted or mutated.
/// </summary>
public class GradeAuditLog : BaseEntity
{
    public Guid SubmissionId { get; set; }
    public Submission? Submission { get; set; }

    public Guid GradedByTeacherId { get; set; }
    public User? GradedByTeacher { get; set; }

    public SubmissionStatus PreviousStatus { get; set; }
    public SubmissionStatus NewStatus { get; set; }
    public int? PreviousMarks { get; set; }
    public int? NewMarks { get; set; }
    public DateTime GradedAt { get; set; }
}
