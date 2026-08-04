using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Application.Academics.Interfaces;

public interface IStudentClassRepository
{
    Task<bool> ExistsAsync(Guid studentId, Guid classId, CancellationToken cancellationToken = default);
    Task AddAsync(StudentClass enrollment, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetClassIdsForStudentAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetStudentIdsForClassAsync(Guid classId, CancellationToken cancellationToken = default);
}
