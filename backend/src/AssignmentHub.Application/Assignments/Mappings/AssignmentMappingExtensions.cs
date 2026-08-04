using AssignmentHub.Application.Assignments.Dtos;
using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Application.Assignments.Mappings;

/// <summary>
/// Assumes ClassSubject, ClassSubject.Class, ClassSubject.Subject and CreatedByTeacher were
/// eager-loaded via .Include()/.ThenInclude() by the caller (see AssignmentService.IncludeAll()).
/// </summary>
public static class AssignmentMappingExtensions
{
    public static AssignmentDto ToDto(this Assignment a) => new(
        a.Id,
        a.Title,
        a.Description,
        a.DeadlineUtc,
        a.MaxMarks,
        a.Status.ToString(),
        a.ClassSubjectId,
        a.ClassSubject!.Class!.Name,
        a.ClassSubject!.Subject!.Name,
        a.CreatedByTeacherId,
        a.CreatedByTeacher!.Name,
        a.CreatedAt);
}
