using AssignmentHub.Application.Settings.Dtos;
using AssignmentHub.Application.Settings.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentHub.Api.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize(Roles = "Admin")]
public class SettingsController : ControllerBase
{
    private readonly ISettingService _settingService;

    public SettingsController(ISettingService settingService)
    {
        _settingService = settingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _settingService.GetAllAsync(cancellationToken));
    }

    [HttpPut]
    public async Task<IActionResult> Upsert([FromBody] UpsertSettingRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _settingService.UpsertAsync(request, cancellationToken));
    }
}
