using AssignmentHub.Application.Assignments.Services;
using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Application.Common.Models;
using AssignmentHub.Application.Submissions.Dtos;
using AssignmentHub.Application.Submissions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentHub.Api.Controllers.Student;

[ApiController]
[Route("api/student/assignments")]
[Authorize(Roles = "Student")]
public class StudentAssignmentsController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;
    private readonly ISubmissionService _submissionService;
    private readonly ICurrentUserService _currentUserService;

    public StudentAssignmentsController(
        IAssignmentService assignmentService,
        ISubmissionService submissionService,
        ICurrentUserService currentUserService)
    {
        _assignmentService = assignmentService;
        _submissionService = submissionService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPublished([FromQuery] PaginationParams pagination, CancellationToken cancellationToken)
    {
        return Ok(await _assignmentService.GetPublishedForStudentAsync(_currentUserService.UserId, pagination, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _assignmentService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitAnswerRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _submissionService.SubmitAsync(id, _currentUserService.UserId, request, cancellationToken));
    }
}
