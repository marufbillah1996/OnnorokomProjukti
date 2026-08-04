using AssignmentHub.Application.Common.Models;
using AssignmentHub.Application.Submissions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentHub.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/submissions")]
[Authorize(Roles = "Admin")]
public class AdminSubmissionsController : ControllerBase
{
    private readonly ISubmissionService _submissionService;

    public AdminSubmissionsController(ISubmissionService submissionService)
    {
        _submissionService = submissionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination, CancellationToken cancellationToken)
    {
        var result = await _submissionService.GetAllForAdminAsync(pagination, cancellationToken);
        return Ok(result);
    }
}
