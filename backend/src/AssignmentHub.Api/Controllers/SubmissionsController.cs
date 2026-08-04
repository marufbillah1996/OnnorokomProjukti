using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Application.Submissions.Dtos;
using AssignmentHub.Application.Submissions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentHub.Api.Controllers;

[ApiController]
[Route("api/submissions")]
[Authorize(Roles = "Teacher")]
public class SubmissionsController : ControllerBase
{
    private readonly ISubmissionService _submissionService;
    private readonly ICurrentUserService _currentUserService;

    public SubmissionsController(ISubmissionService submissionService, ICurrentUserService currentUserService)
    {
        _submissionService = submissionService;
        _currentUserService = currentUserService;
    }

    [HttpPut("{id:guid}/grade")]
    public async Task<IActionResult> Grade(Guid id, [FromBody] GradeSubmissionRequest request, CancellationToken cancellationToken)
    {
        var result = await _submissionService.GradeAsync(id, _currentUserService.UserId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeSubmissionStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _submissionService.ChangeStatusAsync(id, _currentUserService.UserId, request, cancellationToken);
        return Ok(result);
    }
}
