using AssignmentHub.Application.Academics.Dtos;
using AssignmentHub.Application.Academics.Services;
using AssignmentHub.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentHub.Api.Controllers;

[ApiController]
[Route("api/classes")]
[Authorize(Roles = "Admin")]
public class ClassesController : ControllerBase
{
    private readonly IClassService _classService;
    private readonly IClassSubjectService _classSubjectService;

    public ClassesController(IClassService classService, IClassSubjectService classSubjectService)
    {
        _classService = classService;
        _classSubjectService = classSubjectService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination, CancellationToken cancellationToken)
    {
        return Ok(await _classService.GetAllAsync(pagination, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _classService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClassRequest request, CancellationToken cancellationToken)
    {
        var dto = await _classService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClassRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _classService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _classService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{classId:guid}/subjects")]
    public async Task<IActionResult> GetSubjectsForClass(Guid classId, CancellationToken cancellationToken)
    {
        return Ok(await _classSubjectService.GetForClassAsync(classId, cancellationToken));
    }

    [HttpPost("{classId:guid}/subjects/{subjectId:guid}/teachers/{teacherId:guid}")]
    public async Task<IActionResult> AssignTeacher(Guid classId, Guid subjectId, Guid teacherId, CancellationToken cancellationToken)
    {
        return Ok(await _classSubjectService.AssignTeacherAsync(classId, subjectId, teacherId, cancellationToken));
    }

    [HttpPost("{classId:guid}/students/{studentId:guid}")]
    public async Task<IActionResult> EnrollStudent(Guid classId, Guid studentId, CancellationToken cancellationToken)
    {
        await _classSubjectService.EnrollStudentAsync(classId, studentId, cancellationToken);
        return NoContent();
    }
}
