using AssignmentHub.Application.Academics.Dtos;

namespace AssignmentHub.Application.Academics.Services;

public interface IClassSubjectService
{
    Task<ClassSubjectDto> AssignTeacherAsync(Guid classId, Guid subjectId, Guid teacherId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClassSubjectDto>> GetForClassAsync(Guid classId, CancellationToken cancellationToken = default);

    /// <summary>The class/subject pairs a specific teacher is assigned to teach — used to populate
    /// the assignment-creation picker, since a Teacher has no access to the Admin-only class list.</summary>
    Task<IReadOnlyList<ClassSubjectDto>> GetForTeacherAsync(Guid teacherId, CancellationToken cancellationToken = default);

    Task EnrollStudentAsync(Guid classId, Guid studentId, CancellationToken cancellationToken = default);
}
