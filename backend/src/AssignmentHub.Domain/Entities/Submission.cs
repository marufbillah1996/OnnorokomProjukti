using AssignmentHub.Domain.Common;
using AssignmentHub.Domain.Enums;
using AssignmentHub.Domain.Exceptions;

namespace AssignmentHub.Domain.Entities;

public class Submission : BaseEntity, ISoftDelete
{
    public Guid AssignmentId { get; set; }
    public Assignment? Assignment { get; set; }

    public Guid StudentId { get; set; }
    public User? Student { get; set; }

    public string Content { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;
    public int? MarksAwarded { get; set; }
    public string? Feedback { get; set; }

    public bool IsDeleted { get; set; }

    /// <summary>
    /// A submission is student-editable while Pending (not yet graded) or Returned
    /// (explicitly reopened by the teacher for correction). Once Graded, it is locked
    /// until a teacher issues a status override back to Returned.
    /// </summary>
    public bool CanBeModifiedByStudent => Status is SubmissionStatus.Pending or SubmissionStatus.Returned;

    public void UpsertContent(string content, DateTime submittedAtUtc)
    {
        Content = content;
        SubmittedAt = submittedAtUtc;
    }

    public void Grade(int marksAwarded, string? feedback, int maxMarks)
    {
        if (marksAwarded < 0 || marksAwarded > maxMarks)
        {
            throw new InvalidMarksException(marksAwarded, maxMarks);
        }

        MarksAwarded = marksAwarded;
        Feedback = feedback;
        Status = SubmissionStatus.Graded;
    }

    /// <summary>Manual teacher override — reopens a graded submission for the student to correct.</summary>
    public void ReturnToStudent(string? feedback)
    {
        Feedback = feedback;
        Status = SubmissionStatus.Returned;
    }
}
