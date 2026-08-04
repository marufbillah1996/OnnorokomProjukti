namespace AssignmentHub.Domain.Exceptions;

/// <summary>
/// Thrown when a student attempts to submit or update a submission after the assignment's deadline.
/// Mapped to HTTP 409 Conflict by the Api layer's exception middleware.
/// </summary>
public sealed class DeadlinePassedException : DomainException
{
    public DeadlinePassedException(Guid assignmentId, DateTime deadlineUtc)
        : base($"The deadline for assignment '{assignmentId}' passed at {deadlineUtc:O}. Submissions are no longer accepted.")
    {
    }
}
