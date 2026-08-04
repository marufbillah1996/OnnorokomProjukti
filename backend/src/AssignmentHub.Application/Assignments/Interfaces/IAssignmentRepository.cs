using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Application.Assignments.Interfaces;

/// <summary>
/// Marker repository for Assignment — no bespoke members beyond the base CRUD/query contract.
/// Depended on directly (by type) from the Submissions bounded context as well.
/// </summary>
public interface IAssignmentRepository : IRepository<Assignment>
{
}
