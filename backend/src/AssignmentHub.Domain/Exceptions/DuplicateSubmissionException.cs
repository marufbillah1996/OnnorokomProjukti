namespace AssignmentHub.Domain.Exceptions;

/// <summary>
/// Thrown when a student already has a submission row for the given assignment.
/// The unique index on Submissions (AssignmentId, StudentId) is the source of truth;
/// this exception represents the same rule surfaced as a clean, catchable business error.
/// Mapped to HTTP 409 Conflict by the Api layer's exception middleware.
/// </summary>
public sealed class DuplicateSubmissionException : DomainException
{
    public DuplicateSubmissionException(Guid assignmentId, Guid studentId)
        : base($"Student '{studentId}' already has a submission for assignment '{assignmentId}'. Use the update endpoint instead.")
    {
    }
}
