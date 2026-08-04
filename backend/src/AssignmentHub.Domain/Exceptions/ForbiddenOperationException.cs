namespace AssignmentHub.Domain.Exceptions;

/// <summary>
/// Thrown when a user attempts an operation on a resource they do not own or are not
/// assigned to (e.g. a Teacher editing another Teacher's assignment). This is the
/// exception surfaced by resource-based authorization checks in the Application layer.
/// Mapped to HTTP 403 Forbidden by the Api layer's exception middleware.
/// </summary>
public sealed class ForbiddenOperationException : DomainException
{
    public ForbiddenOperationException(string message) : base(message)
    {
    }
}
