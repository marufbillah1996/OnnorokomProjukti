using AssignmentHub.Application.Academics.Services;
using AssignmentHub.Application.Assignments.Dtos;
using AssignmentHub.Application.Assignments.Services;
using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Application.Common.Models;
using AssignmentHub.Application.Submissions.Services;
using AssignmentHub.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentHub.Api.Controllers;

[ApiController]
[Route("api/assignments")]
[Authorize(Roles = "Teacher")]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;
    private readonly ISubmissionService _submissionService;
    private readonly IClassSubjectService _classSubjectService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;

    public AssignmentsController(
        IAssignmentService assignmentService,
        ISubmissionService submissionService,
        IClassSubjectService classSubjectService,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService)
    {
        _assignmentService = assignmentService;
        _submissionService = submissionService;
        _classSubjectService = classSubjectService;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMine([FromQuery] PaginationParams pagination, CancellationToken cancellationToken)
    {
        return Ok(await _assignmentService.GetAllForTeacherAsync(_currentUserService.UserId, pagination, cancellationToken));
    }

    /// <summary>
    /// The class/subject pairs this teacher is assigned to teach — used to populate the "Class"
    /// picker when creating an assignment. A Teacher has no access to the Admin-only
    /// GET /api/classes, so the create-assignment form needs a teacher-scoped source for this data.
    /// </summary>
    [HttpGet("class-subjects")]
    public async Task<IActionResult> GetMyClassSubjects(CancellationToken cancellationToken)
    {
        return Ok(await _classSubjectService.GetForTeacherAsync(_currentUserService.UserId, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var authResult = await _authorizationService.AuthorizeAsync(User, id, new AssignmentOwnershipRequirement());
        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        return Ok(await _assignmentService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAssignmentRequest request, CancellationToken cancellationToken)
    {
        var dto = await _assignmentService.CreateAsync(_currentUserService.UserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAssignmentRequest request, CancellationToken cancellationToken)
    {
        var authResult = await _authorizationService.AuthorizeAsync(User, id, new AssignmentOwnershipRequirement());
        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        return Ok(await _assignmentService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var authResult = await _authorizationService.AuthorizeAsync(User, id, new AssignmentOwnershipRequirement());
        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        await _assignmentService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken cancellationToken)
    {
        var authResult = await _authorizationService.AuthorizeAsync(User, id, new AssignmentOwnershipRequirement());
        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        return Ok(await _assignmentService.PublishAsync(id, cancellationToken));
    }

    [HttpGet("{id:guid}/submissions")]
    public async Task<IActionResult> GetSubmissions(Guid id, [FromQuery] PaginationParams pagination, CancellationToken cancellationToken)
    {
        var authResult = await _authorizationService.AuthorizeAsync(User, id, new AssignmentOwnershipRequirement());
        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        return Ok(await _submissionService.GetForAssignmentAsync(id, pagination, cancellationToken));
    }
}
