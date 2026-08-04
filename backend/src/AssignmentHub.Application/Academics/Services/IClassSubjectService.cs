using AssignmentHub.Application.Academics.Dtos;

namespace AssignmentHub.Application.Academics.Services;

public interface IClassSubjectService
{
    Task<ClassSubjectDto> AssignTeacherAsync(Guid classId, Guid subjectId, Guid teacherId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClassSubjectDto>> GetForClassAsync(Guid classId, CancellationToken cancellationToken = default);
    Task EnrollStudentAsync(Guid classId, Guid studentId, CancellationToken cancellationToken = default);
}
