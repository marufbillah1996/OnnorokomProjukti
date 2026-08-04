using AssignmentHub.Domain.Common;
using AssignmentHub.Domain.Enums;

namespace AssignmentHub.Domain.Entities;

public class Assignment : BaseEntity, ISoftDelete
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>Always stored and compared in UTC — never the authoritative deadline check on the client.</summary>
    public DateTime DeadlineUtc { get; set; }

    public int MaxMarks { get; set; }
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;

    public Guid ClassSubjectId { get; set; }
    public ClassSubject? ClassSubject { get; set; }

    public Guid CreatedByTeacherId { get; set; }
    public User? CreatedByTeacher { get; set; }

    public bool IsDeleted { get; set; }

    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public bool IsDraft => Status == AssignmentStatus.Draft;

    public void Publish() => Status = AssignmentStatus.Published;

    /// <summary>True while the assignment is published and the deadline has not yet passed.</summary>
    public bool AcceptsSubmissions(DateTime utcNow) =>
        Status == AssignmentStatus.Published && utcNow <= DeadlineUtc;

    public bool IsPastDeadline(DateTime utcNow) => utcNow > DeadlineUtc;
}
