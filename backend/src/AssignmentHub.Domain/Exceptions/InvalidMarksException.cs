namespace AssignmentHub.Domain.Exceptions;

/// <summary>
/// Thrown when a teacher attempts to award marks outside the valid range for an assignment.
/// Mapped to HTTP 400 Bad Request by the Api layer's exception middleware.
/// </summary>
public sealed class InvalidMarksException : DomainException
{
    public InvalidMarksException(int marksAwarded, int maxMarks)
        : base($"Marks awarded ({marksAwarded}) must be between 0 and the assignment's maximum marks ({maxMarks}).")
    {
    }
}
