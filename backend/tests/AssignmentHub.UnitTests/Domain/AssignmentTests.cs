using AssignmentHub.Domain.Entities;
using AssignmentHub.Domain.Enums;
using FluentAssertions;

namespace AssignmentHub.UnitTests.Domain;

public class AssignmentTests
{
    [Fact]
    public void Publish_TransitionsStatusFromDraftToPublished()
    {
        var assignment = new Assignment { Status = AssignmentStatus.Draft };

        assignment.Publish();

        assignment.Status.Should().Be(AssignmentStatus.Published);
    }

    [Fact]
    public void AcceptsSubmissions_WhenDraftAndBeforeDeadline_ReturnsFalse()
    {
        var deadline = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var assignment = new Assignment { Status = AssignmentStatus.Draft, DeadlineUtc = deadline };

        assignment.AcceptsSubmissions(deadline.AddDays(-1)).Should().BeFalse();
    }

    [Fact]
    public void AcceptsSubmissions_WhenPublishedAndBeforeDeadline_ReturnsTrue()
    {
        var deadline = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var assignment = new Assignment { Status = AssignmentStatus.Published, DeadlineUtc = deadline };

        assignment.AcceptsSubmissions(deadline.AddMinutes(-1)).Should().BeTrue();
    }

    [Fact]
    public void AcceptsSubmissions_WhenPublishedAndUtcNowExactlyEqualsDeadline_ReturnsTrue()
    {
        // Boundary check: the comparison is "<=", so utcNow == DeadlineUtc must still accept submissions.
        var deadline = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var assignment = new Assignment { Status = AssignmentStatus.Published, DeadlineUtc = deadline };

        assignment.AcceptsSubmissions(deadline).Should().BeTrue();
    }

    [Fact]
    public void AcceptsSubmissions_WhenPublishedAndOneTickAfterDeadline_ReturnsFalse()
    {
        var deadline = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var assignment = new Assignment { Status = AssignmentStatus.Published, DeadlineUtc = deadline };

        assignment.AcceptsSubmissions(deadline.AddTicks(1)).Should().BeFalse();
    }

    [Fact]
    public void IsPastDeadline_WhenUtcNowExactlyEqualsDeadline_ReturnsFalse()
    {
        var deadline = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var assignment = new Assignment { DeadlineUtc = deadline };

        assignment.IsPastDeadline(deadline).Should().BeFalse();
    }

    [Fact]
    public void IsPastDeadline_WhenOneTickAfterDeadline_ReturnsTrue()
    {
        var deadline = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var assignment = new Assignment { DeadlineUtc = deadline };

        assignment.IsPastDeadline(deadline.AddTicks(1)).Should().BeTrue();
    }
}
