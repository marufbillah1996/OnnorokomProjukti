using System.Security.Claims;
using AssignmentHub.Application.Assignments.Services;
using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Domain.Enums;
using AssignmentHub.Infrastructure.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;

namespace AssignmentHub.UnitTests.Authorization;

public class AssignmentOwnershipHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenCallerIsAdmin_SucceedsRegardlessOfOwnership()
    {
        var mockAssignmentService = new Mock<IAssignmentService>();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(s => s.Role).Returns(UserRole.Admin);

        // Deliberately wired to return false to prove the Admin bypass truly short-circuits
        // before IsOwnedByTeacherAsync's result could matter.
        mockAssignmentService
            .Setup(s => s.IsOwnedByTeacherAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new AssignmentOwnershipHandler(mockAssignmentService.Object, mockCurrentUserService.Object);
        var context = new AuthorizationHandlerContext(
            new[] { new AssignmentOwnershipRequirement() },
            new ClaimsPrincipal(),
            Guid.NewGuid());

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenTeacherOwnsAssignment_Succeeds()
    {
        var teacherId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();

        var mockAssignmentService = new Mock<IAssignmentService>();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(s => s.Role).Returns(UserRole.Teacher);
        mockCurrentUserService.Setup(s => s.UserId).Returns(teacherId);
        mockAssignmentService
            .Setup(s => s.IsOwnedByTeacherAsync(assignmentId, teacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new AssignmentOwnershipHandler(mockAssignmentService.Object, mockCurrentUserService.Object);
        var context = new AuthorizationHandlerContext(
            new[] { new AssignmentOwnershipRequirement() },
            new ClaimsPrincipal(),
            assignmentId);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenTeacherDoesNotOwnAssignment_DoesNotSucceed()
    {
        var teacherId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();

        var mockAssignmentService = new Mock<IAssignmentService>();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(s => s.Role).Returns(UserRole.Teacher);
        mockCurrentUserService.Setup(s => s.UserId).Returns(teacherId);
        mockAssignmentService
            .Setup(s => s.IsOwnedByTeacherAsync(assignmentId, teacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new AssignmentOwnershipHandler(mockAssignmentService.Object, mockCurrentUserService.Object);
        var context = new AuthorizationHandlerContext(
            new[] { new AssignmentOwnershipRequirement() },
            new ClaimsPrincipal(),
            assignmentId);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }
}
