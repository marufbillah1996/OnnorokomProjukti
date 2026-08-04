namespace AssignmentHub.Domain.Exceptions;

/// <summary>
/// Base type for all business-rule violations raised from within the Domain layer.
/// The Api layer's global exception middleware maps subclasses to specific HTTP status codes.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}
