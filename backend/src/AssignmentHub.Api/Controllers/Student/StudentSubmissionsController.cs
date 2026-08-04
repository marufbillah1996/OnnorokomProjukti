using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Application.Common.Models;
using AssignmentHub.Application.Submissions.Dtos;
using AssignmentHub.Application.Submissions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentHub.Api.Controllers.Student;

[ApiController]
[Route("api/student/submissions")]
[Authorize(Roles = "Student")]
public class StudentSubmissionsController : ControllerBase
{
    private readonly ISubmissionService _submissionService;
    private readonly ICurrentUserService _currentUserService;

    public StudentSubmissionsController(ISubmissionService submissionService, ICurrentUserService currentUserService)
    {
        _submissionService = submissionService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMine([FromQuery] PaginationParams pagination, CancellationToken cancellationToken)
    {
        var result = await _submissionService.GetForStudentAsync(_currentUserService.UserId, pagination, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SubmitAnswerRequest request, CancellationToken cancellationToken)
    {
        var result = await _submissionService.UpdateAsync(id, _currentUserService.UserId, request, cancellationToken);
        return Ok(result);
    }
}
