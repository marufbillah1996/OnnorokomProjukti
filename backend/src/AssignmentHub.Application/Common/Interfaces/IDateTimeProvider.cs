namespace AssignmentHub.Application.Common.Interfaces;

/// <summary>
/// Abstraction over "now", always UTC. Every deadline comparison in the system goes through
/// this instead of DateTime.UtcNow directly, so unit tests can freeze/control time deterministically
/// (e.g. to test the exact deadline boundary).
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
