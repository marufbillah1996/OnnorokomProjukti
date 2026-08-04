using AssignmentHub.Application.Academics.Interfaces;
using AssignmentHub.Application.Assignments.Dtos;
using AssignmentHub.Application.Assignments.Interfaces;
using AssignmentHub.Application.Assignments.Services;
using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Application.Common.Models;
using AssignmentHub.Application.Notifications.Interfaces;
using AssignmentHub.Domain.Entities;
using AssignmentHub.Domain.Enums;
using AssignmentHub.Domain.Exceptions;
using FluentAssertions;
using MockQueryable;
using Moq;

namespace AssignmentHub.UnitTests.Assignments;

public class AssignmentServiceTests
{
    private sealed class Harness
    {
        public Mock<IAssignmentRepository> AssignmentRepository { get; } = new();
        public Mock<IClassSubjectRepository> ClassSubjectRepository { get; } = new();
        public Mock<IStudentClassRepository> StudentClassRepository { get; } = new();
        public Mock<INotificationRepository> NotificationRepository { get; } = new();
        public Mock<IRealtimeNotifier> RealtimeNotifier { get; } = new();
        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public Mock<IDateTimeProvider> DateTimeProvider { get; } = new();

        public AssignmentService BuildSut() => new(
            AssignmentRepository.Object,
            ClassSubjectRepository.Object,
            StudentClassRepository.Object,
            NotificationRepository.Object,
            RealtimeNotifier.Object,
            UnitOfWork.Object,
            DateTimeProvider.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenClassSubjectDoesNotExist_ThrowsKeyNotFoundException()
    {
        var harness = new Harness();
        harness.ClassSubjectRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClassSubject?)null);

        var sut = harness.BuildSut();
        var request = new CreateAssignmentRequest("Title", "Description", DateTime.UtcNow.AddDays(7), 100, Guid.NewGuid());

        await FluentActions.Invoking(() => sut.CreateAsync(Guid.NewGuid(), request))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_WhenTeacherDoesNotOwnClassSubject_ThrowsForbiddenOperationException()
    {
        var harness = new Harness();
        var classSubject = new ClassSubject { Id = Guid.NewGuid(), TeacherId = Guid.NewGuid() };
        harness.ClassSubjectRepository
            .Setup(r => r.GetByIdAsync(classSubject.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(classSubject);

        var sut = harness.BuildSut();
        var request = new CreateAssignmentRequest("Title", "Description", DateTime.UtcNow.AddDays(7), 100, classSubject.Id);
        var differentTeacherId = Guid.NewGuid();

        await FluentActions.Invoking(() => sut.CreateAsync(differentTeacherId, request))
            .Should().ThrowAsync<ForbiddenOperationException>();
    }

    [Fact]
    public async Task CreateAsync_WhenTeacherOwnsClassSubject_AddsDraftAssignmentAndSaves()
    {
        var harness = new Harness();
        var teacherId = Guid.NewGuid();
        var classSubject = new ClassSubject
        {
            Id = Guid.NewGuid(),
            TeacherId = teacherId,
            Class = new Class { Name = "Class A" },
            Subject = new Subject { Name = "Math" }
        };
        harness.ClassSubjectRepository
            .Setup(r => r.GetByIdAsync(classSubject.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(classSubject);

        // The service re-fetches the newly created assignment via Query()+Include()+FirstOrDefaultAsync
        // to build its return DTO. Capture the entity added via AddAsync, hydrate the navigation
        // properties ToDto() depends on, and back Query() with it so that final re-fetch succeeds
        // without us needing to assert on the DTO shape itself.
        var backing = new List<Assignment>();
        harness.AssignmentRepository
            .Setup(r => r.AddAsync(It.IsAny<Assignment>(), It.IsAny<CancellationToken>()))
            .Callback<Assignment, CancellationToken>((a, _) =>
            {
                a.ClassSubject = classSubject;
                a.CreatedByTeacher = new User { Id = teacherId, Name = "Teacher" };
                backing.Add(a);
            })
            .Returns(Task.CompletedTask);
        harness.AssignmentRepository
            .Setup(r => r.Query())
            .Returns(() => backing.BuildMock());

        var sut = harness.BuildSut();
        var request = new CreateAssignmentRequest("Title", "Description", DateTime.UtcNow.AddDays(7), 100, classSubject.Id);

        await sut.CreateAsync(teacherId, request);

        harness.AssignmentRepository.Verify(r => r.AddAsync(
                It.Is<Assignment>(a =>
                    a.Status == AssignmentStatus.Draft &&
                    a.Title == request.Title &&
                    a.ClassSubjectId == classSubject.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
        harness.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetPublishedForStudentAsync_ReturnsOnlyPublishedAssignmentsInStudentsEnrolledClasses()
    {
        var harness = new Harness();
        var studentId = Guid.NewGuid();
        var classId = Guid.NewGuid();

        harness.StudentClassRepository
            .Setup(r => r.GetClassIdsForStudentAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid> { classId });

        var classSubject = new ClassSubject
        {
            Id = Guid.NewGuid(),
            ClassId = classId,
            Class = new Class { Name = "Class A" },
            Subject = new Subject { Name = "Math" }
        };
        var teacher = new User { Id = Guid.NewGuid(), Name = "Teacher" };

        var publishedAssignment = new Assignment
        {
            Id = Guid.NewGuid(),
            Status = AssignmentStatus.Published,
            ClassSubjectId = classSubject.Id,
            ClassSubject = classSubject,
            CreatedByTeacher = teacher,
            Title = "Published Assignment"
        };
        var draftAssignment = new Assignment
        {
            Id = Guid.NewGuid(),
            Status = AssignmentStatus.Draft,
            ClassSubjectId = classSubject.Id,
            ClassSubject = classSubject,
            CreatedByTeacher = teacher,
            Title = "Draft Assignment"
        };

        var data = new List<Assignment> { publishedAssignment, draftAssignment }.BuildMock();
        harness.AssignmentRepository.Setup(r => r.Query()).Returns(data);

        var sut = harness.BuildSut();

        var result = await sut.GetPublishedForStudentAsync(studentId, new PaginationParams());

        result.Items.Should().ContainSingle();
        result.Items.Single().Id.Should().Be(publishedAssignment.Id);
    }

    [Fact]
    public async Task IsOwnedByTeacherAsync_WhenAssignmentBelongsToTeacher_ReturnsTrue()
    {
        var harness = new Harness();
        var teacherId = Guid.NewGuid();
        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            CreatedByTeacherId = teacherId,
            ClassSubject = new ClassSubject { TeacherId = teacherId }
        };
        var data = new List<Assignment> { assignment }.BuildMock();
        harness.AssignmentRepository.Setup(r => r.Query()).Returns(data);

        var sut = harness.BuildSut();

        var result = await sut.IsOwnedByTeacherAsync(assignment.Id, teacherId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsOwnedByTeacherAsync_WhenAssignmentBelongsToDifferentTeacher_ReturnsFalse()
    {
        var harness = new Harness();
        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            CreatedByTeacherId = Guid.NewGuid(),
            ClassSubject = new ClassSubject { TeacherId = Guid.NewGuid() }
        };
        var data = new List<Assignment> { assignment }.BuildMock();
        harness.AssignmentRepository.Setup(r => r.Query()).Returns(data);

        var sut = harness.BuildSut();

        var result = await sut.IsOwnedByTeacherAsync(assignment.Id, Guid.NewGuid());

        result.Should().BeFalse();
    }
}
