using AssignmentHub.Application.Assignments.Services;
using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace AssignmentHub.Infrastructure.Authorization;

public class AssignmentOwnershipHandler : AuthorizationHandler<AssignmentOwnershipRequirement, Guid>
{
    private readonly IAssignmentService _assignmentService;
    private readonly ICurrentUserService _currentUserService;

    public AssignmentOwnershipHandler(IAssignmentService assignmentService, ICurrentUserService currentUserService)
    {
        _assignmentService = assignmentService;
        _currentUserService = currentUserService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AssignmentOwnershipRequirement requirement,
        Guid resource)
    {
        if (_currentUserService.Role == UserRole.Admin)
        {
            context.Succeed(requirement);
            return;
        }

        var isOwner = await _assignmentService.IsOwnedByTeacherAsync(resource, _currentUserService.UserId);
        if (isOwner)
        {
            context.Succeed(requirement);
        }
    }
}
