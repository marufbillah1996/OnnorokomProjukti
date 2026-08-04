using AssignmentHub.Application.Academics.Dtos;
using AssignmentHub.Application.Academics.Interfaces;
using AssignmentHub.Application.Academics.Mappings;
using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Application.Common.Models;
using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Application.Academics.Services;

public class ClassService : IClassService
{
    private readonly IClassRepository _classRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClassService(IClassRepository classRepository, IUnitOfWork unitOfWork)
    {
        _classRepository = classRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedList<ClassDto>> GetAllAsync(PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = _classRepository.Query();

        if (!string.IsNullOrWhiteSpace(pagination.Search))
        {
            query = query.Where(c => c.Name.Contains(pagination.Search));
        }

        query = pagination.IsDescending
            ? query.OrderByDescending(c => c.Name)
            : query.OrderBy(c => c.Name);

        var paginated = await PaginatedList<Class>.CreateAsync(query, pagination.Page, pagination.PageSize, cancellationToken);
        return paginated.Map(c => c.ToDto());
    }

    public async Task<ClassDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _classRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Class not found.");

        return entity.ToDto();
    }

    public async Task<ClassDto> CreateAsync(CreateClassRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Class
        {
            Name = request.Name,
            Description = request.Description
        };

        await _classRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task<ClassDto> UpdateAsync(Guid id, UpdateClassRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _classRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Class not found.");

        entity.Name = request.Name;
        entity.Description = request.Description;

        _classRepository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _classRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Class not found.");

        entity.IsDeleted = true;

        _classRepository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
