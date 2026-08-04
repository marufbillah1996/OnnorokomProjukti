using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Application.Academics.Interfaces;

public interface IClassSubjectRepository : IRepository<ClassSubject>
{
    Task<ClassSubject?> GetAsync(Guid classId, Guid subjectId, CancellationToken cancellationToken = default);
}
