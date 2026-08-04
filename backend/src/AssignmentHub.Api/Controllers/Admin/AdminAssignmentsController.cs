using AssignmentHub.Application.Assignments.Services;
using AssignmentHub.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentHub.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/assignments")]
[Authorize(Roles = "Admin")]
public class AdminAssignmentsController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;

    public AdminAssignmentsController(IAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination, CancellationToken cancellationToken)
    {
        return Ok(await _assignmentService.GetAllForAdminAsync(pagination, cancellationToken));
    }
}
