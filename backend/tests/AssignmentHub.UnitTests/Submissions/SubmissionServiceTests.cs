using AssignmentHub.Application.Assignments.Interfaces;
using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Application.Notifications.Interfaces;
using AssignmentHub.Application.Submissions.Dtos;
using AssignmentHub.Application.Submissions.Interfaces;
using AssignmentHub.Application.Submissions.Services;
using AssignmentHub.Domain.Entities;
using AssignmentHub.Domain.Enums;
using AssignmentHub.Domain.Exceptions;
using FluentAssertions;
using MockQueryable;
using Moq;

namespace AssignmentHub.UnitTests.Submissions;

public class SubmissionServiceTests
{
    private sealed class Harness
    {
        public Mock<ISubmissionRepository> SubmissionRepository { get; } = new();
        public Mock<IGradeAuditLogRepository> GradeAuditLogRepository { get; } = new();
        public Mock<IAssignmentRepository> AssignmentRepository { get; } = new();
        public Mock<INotificationRepository> NotificationRepository { get; } = new();
        public Mock<IRealtimeNotifier> RealtimeNotifier { get; } = new();
        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public Mock<IDateTimeProvider> DateTimeProvider { get; } = new();

        public SubmissionService BuildSut() => new(
            SubmissionRepository.Object,
            GradeAuditLogRepository.Object,
            AssignmentRepository.Object,
            NotificationRepository.Object,
            RealtimeNotifier.Object,
            UnitOfWork.Object,
            DateTimeProvider.Object);
    }

    [Fact]
    public async Task SubmitAsync_WhenAssignmentDeadlineHasPassed_ThrowsDeadlinePassedException()
    {
        var harness = new Harness();
        var now = new DateTime(2026, 8, 4, 12, 0, 0, DateTimeKind.Utc);
        harness.DateTimeProvider.Setup(p => p.UtcNow).Returns(now);

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            Status = AssignmentStatus.Published,
            DeadlineUtc = now.AddDays(-1)
        };
        harness.AssignmentRepository
            .Setup(r => r.GetByIdAsync(assignment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        var sut = harness.BuildSut();
        var request = new SubmitAnswerRequest("My answer");

        await FluentActions.Invoking(() => sut.SubmitAsync(assignment.Id, Guid.NewGuid(), request))
            .Should().ThrowAsync<DeadlinePassedException>();
    }

    [Fact]
    public async Task SubmitAsync_WhenSubmissionAlreadyExistsForStudent_ThrowsDuplicateSubmissionException()
    {
        var harness = new Harness();
        var now = new DateTime(2026, 8, 4, 12, 0, 0, DateTimeKind.Utc);
        harness.DateTimeProvider.Setup(p => p.UtcNow).Returns(now);

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            Status = AssignmentStatus.Published,
            DeadlineUtc = now.AddDays(1)
        };
        harness.AssignmentRepository
            .Setup(r => r.GetByIdAsync(assignment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        var studentId = Guid.NewGuid();
        harness.SubmissionRepository
            .Setup(r => r.GetByAssignmentAndStudentAsync(assignment.Id, studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Submission { Id = Guid.NewGuid(), AssignmentId = assignment.Id, StudentId = studentId });

        var sut = harness.BuildSut();
        var request = new SubmitAnswerRequest("My answer");

        await FluentActions.Invoking(() => sut.SubmitAsync(assignment.Id, studentId, request))
            .Should().ThrowAsync<DuplicateSubmissionException>();
    }

    [Fact]
    public async Task SubmitAsync_WhenAssignmentIsOpenAndNoExistingSubmission_AddsPendingSubmissionAndSaves()
    {
        var harness = new Harness();
        var now = new DateTime(2026, 8, 4, 12, 0, 0, DateTimeKind.Utc);
        harness.DateTimeProvider.Setup(p => p.UtcNow).Returns(now);

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            Status = AssignmentStatus.Published,
            DeadlineUtc = now.AddDays(1),
            MaxMarks = 100,
            Title = "Homework"
        };
        harness.AssignmentRepository
            .Setup(r => r.GetByIdAsync(assignment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        var studentId = Guid.NewGuid();
        harness.SubmissionRepository
            .Setup(r => r.GetByAssignmentAndStudentAsync(assignment.Id, studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Submission?)null);

        // SubmitAsync re-fetches the newly created submission via Query()+Include()+FirstOrDefaultAsync
        // to build its return DTO. Capture the entity added via AddAsync, hydrate the navigation
        // properties ToDto() depends on, and back Query() with it so that final re-fetch succeeds
        // without us needing to assert on the DTO shape itself.
        var backing = new List<Submission>();
        harness.SubmissionRepository
            .Setup(r => r.AddAsync(It.IsAny<Submission>(), It.IsAny<CancellationToken>()))
            .Callback<Submission, CancellationToken>((s, _) =>
            {
                s.Assignment = assignment;
                s.Student = new User { Id = studentId, Name = "Student" };
                backing.Add(s);
            })
            .Returns(Task.CompletedTask);
        harness.SubmissionRepository
            .Setup(r => r.Query())
            .Returns(() => backing.BuildMock());

        var sut = harness.BuildSut();
        var request = new SubmitAnswerRequest("My answer");

        await sut.SubmitAsync(assignment.Id, studentId, request);

        harness.SubmissionRepository.Verify(r => r.AddAsync(
                It.Is<Submission>(s =>
                    s.Status == SubmissionStatus.Pending &&
                    s.Content == request.Content &&
                    s.AssignmentId == assignment.Id &&
                    s.StudentId == studentId),
                It.IsAny<CancellationToken>()),
            Times.Once);
        harness.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateAsync_WhenSubmissionBelongsToDifferentStudent_ThrowsForbiddenOperationException()
    {
        var harness = new Harness();
        var submission = new Submission { Id = Guid.NewGuid(), StudentId = Guid.NewGuid(), Status = SubmissionStatus.Pending };
        harness.SubmissionRepository
            .Setup(r => r.GetByIdAsync(submission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);

        var sut = harness.BuildSut();
        var request = new SubmitAnswerRequest("Updated answer");

        await FluentActions.Invoking(() => sut.UpdateAsync(submission.Id, Guid.NewGuid(), request))
            .Should().ThrowAsync<ForbiddenOperationException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenSubmissionIsGraded_ThrowsInvalidOperationException()
    {
        var harness = new Harness();
        var studentId = Guid.NewGuid();
        var submission = new Submission { Id = Guid.NewGuid(), StudentId = studentId, Status = SubmissionStatus.Graded };
        harness.SubmissionRepository
            .Setup(r => r.GetByIdAsync(submission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);

        var sut = harness.BuildSut();
        var request = new SubmitAnswerRequest("Updated answer");

        await FluentActions.Invoking(() => sut.UpdateAsync(submission.Id, studentId, request))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GradeAsync_WhenTeacherDoesNotOwnAssignment_ThrowsForbiddenOperationException()
    {
        var harness = new Harness();
        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            CreatedByTeacherId = Guid.NewGuid(),
            ClassSubject = new ClassSubject { TeacherId = Guid.NewGuid() },
            MaxMarks = 100
        };
        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            AssignmentId = assignment.Id,
            Assignment = assignment,
            Status = SubmissionStatus.Pending
        };

        harness.SubmissionRepository
            .Setup(r => r.GetByIdAsync(submission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);

        // IsGradableByTeacherAsync queries submissionRepository.Query().Include(...).AnyAsync(...).
        var data = new List<Submission> { submission }.BuildMock();
        harness.SubmissionRepository.Setup(r => r.Query()).Returns(data);

        var sut = harness.BuildSut();
        var request = new GradeSubmissionRequest(80, "Good job");

        await FluentActions.Invoking(() => sut.GradeAsync(submission.Id, Guid.NewGuid(), request))
            .Should().ThrowAsync<ForbiddenOperationException>();
    }

    [Fact]
    public async Task GradeAsync_WhenMarksAwardedExceedsAssignmentMaxMarks_ThrowsInvalidMarksException()
    {
        var harness = new Harness();
        var teacherId = Guid.NewGuid();
        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            CreatedByTeacherId = teacherId,
            ClassSubject = new ClassSubject { TeacherId = teacherId },
            MaxMarks = 50
        };
        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            AssignmentId = assignment.Id,
            Assignment = assignment,
            Status = SubmissionStatus.Pending
        };

        harness.SubmissionRepository
            .Setup(r => r.GetByIdAsync(submission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);
        harness.AssignmentRepository
            .Setup(r => r.GetByIdAsync(assignment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        var data = new List<Submission> { submission }.BuildMock();
        harness.SubmissionRepository.Setup(r => r.Query()).Returns(data);

        var sut = harness.BuildSut();
        var request = new GradeSubmissionRequest(100, "Great work");

        await FluentActions.Invoking(() => sut.GradeAsync(submission.Id, teacherId, request))
            .Should().ThrowAsync<InvalidMarksException>();
    }
}
