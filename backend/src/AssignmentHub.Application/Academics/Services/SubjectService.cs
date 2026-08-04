using AssignmentHub.Application.Academics.Dtos;
using AssignmentHub.Application.Academics.Interfaces;
using AssignmentHub.Application.Academics.Mappings;
using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Application.Common.Models;
using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Application.Academics.Services;

public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubjectService(ISubjectRepository subjectRepository, IUnitOfWork unitOfWork)
    {
        _subjectRepository = subjectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedList<SubjectDto>> GetAllAsync(PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = _subjectRepository.Query();

        if (!string.IsNullOrWhiteSpace(pagination.Search))
        {
            query = query.Where(s => s.Name.Contains(pagination.Search) || s.Code.Contains(pagination.Search));
        }

        query = pagination.IsDescending
            ? query.OrderByDescending(s => s.Name)
            : query.OrderBy(s => s.Name);

        var paginated = await PaginatedList<Subject>.CreateAsync(query, pagination.Page, pagination.PageSize, cancellationToken);
        return paginated.Map(s => s.ToDto());
    }

    public async Task<SubjectDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _subjectRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Subject not found.");

        return entity.ToDto();
    }

    public async Task<SubjectDto> CreateAsync(CreateSubjectRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Subject
        {
            Name = request.Name,
            Code = request.Code
        };

        await _subjectRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task<SubjectDto> UpdateAsync(Guid id, UpdateSubjectRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _subjectRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Subject not found.");

        entity.Name = request.Name;
        entity.Code = request.Code;

        _subjectRepository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _subjectRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Subject not found.");

        entity.IsDeleted = true;

        _subjectRepository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
