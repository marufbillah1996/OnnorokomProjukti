using AssignmentHub.Application.Academics.Dtos;
using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Application.Academics.Mappings;

public static class ClassSubjectMappingExtensions
{
    /// <summary>
    /// Assumes the Class, Subject and Teacher navigations were eager-loaded by the caller
    /// (via .Include) — this mapper does not itself query the database.
    /// </summary>
    public static ClassSubjectDto ToDto(this ClassSubject cs) => new(
        cs.Id,
        cs.ClassId,
        cs.Class!.Name,
        cs.SubjectId,
        cs.Subject!.Name,
        cs.TeacherId,
        cs.Teacher!.Name);
}
