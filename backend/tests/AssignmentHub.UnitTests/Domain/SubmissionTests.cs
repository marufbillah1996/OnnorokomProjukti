using AssignmentHub.Domain.Entities;
using AssignmentHub.Domain.Enums;
using AssignmentHub.Domain.Exceptions;
using FluentAssertions;

namespace AssignmentHub.UnitTests.Domain;

public class SubmissionTests
{
    [Fact]
    public void Grade_WhenMarksAwardedIsZero_SucceedsAndSetsGradedState()
    {
        var submission = new Submission();

        submission.Grade(0, "Needs improvement", maxMarks: 50);

        submission.Status.Should().Be(SubmissionStatus.Graded);
        submission.MarksAwarded.Should().Be(0);
        submission.Feedback.Should().Be("Needs improvement");
    }

    [Fact]
    public void Grade_WhenMarksAwardedEqualsMaxMarks_SucceedsAndSetsGradedState()
    {
        var submission = new Submission();

        submission.Grade(50, "Perfect score", maxMarks: 50);

        submission.Status.Should().Be(SubmissionStatus.Graded);
        submission.MarksAwarded.Should().Be(50);
        submission.Feedback.Should().Be("Perfect score");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(51)]
    public void Grade_WhenMarksAwardedIsOutsideValidRange_ThrowsInvalidMarksException(int marksAwarded)
    {
        var submission = new Submission();

        var act = () => submission.Grade(marksAwarded, "Feedback", maxMarks: 50);

        act.Should().Throw<InvalidMarksException>();
    }

    [Fact]
    public void ReturnToStudent_SetsStatusReturnedAndUpdatesFeedback()
    {
        var submission = new Submission { Status = SubmissionStatus.Graded, MarksAwarded = 40, Feedback = "Old feedback" };

        submission.ReturnToStudent("Please revise question 3");

        submission.Status.Should().Be(SubmissionStatus.Returned);
        submission.Feedback.Should().Be("Please revise question 3");
    }

    [Theory]
    [InlineData(SubmissionStatus.Pending, true)]
    [InlineData(SubmissionStatus.Returned, true)]
    [InlineData(SubmissionStatus.Graded, false)]
    public void CanBeModifiedByStudent_ReflectsCurrentStatus(SubmissionStatus status, bool expected)
    {
        var submission = new Submission { Status = status };

        submission.CanBeModifiedByStudent.Should().Be(expected);
    }
}
