using AssignmentHub.Application.Common.Interfaces;

namespace AssignmentHub.Infrastructure.Identity;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
