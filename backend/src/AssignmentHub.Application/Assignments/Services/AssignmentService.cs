using System.Text.Json;
using AssignmentHub.Application.Academics.Interfaces;
using AssignmentHub.Application.Assignments.Dtos;
using AssignmentHub.Application.Assignments.Interfaces;
using AssignmentHub.Application.Assignments.Mappings;
using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Application.Common.Models;
using AssignmentHub.Application.Notifications.Interfaces;
using AssignmentHub.Domain.Entities;
using AssignmentHub.Domain.Enums;
using AssignmentHub.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AssignmentHub.Application.Assignments.Services;

public class AssignmentService : IAssignmentService
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IClassSubjectRepository _classSubjectRepository;
    private readonly IStudentClassRepository _studentClassRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IRealtimeNotifier _realtimeNotifier;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AssignmentService(
        IAssignmentRepository assignmentRepository,
        IClassSubjectRepository classSubjectRepository,
        IStudentClassRepository studentClassRepository,
        INotificationRepository notificationRepository,
        IRealtimeNotifier realtimeNotifier,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _assignmentRepository = assignmentRepository;
        _classSubjectRepository = classSubjectRepository;
        _studentClassRepository = studentClassRepository;
        _notificationRepository = notificationRepository;
        _realtimeNotifier = realtimeNotifier;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<PaginatedList<AssignmentDto>> GetAllForAdminAsync(PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = ApplySearchAndSort(IncludeAll(), pagination);
        var page = await PaginatedList<Assignment>.CreateAsync(query, pagination.Page, pagination.PageSize, cancellationToken);
        return page.Map(a => a.ToDto());
    }

    public async Task<PaginatedList<AssignmentDto>> GetAllForTeacherAsync(Guid teacherId, PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = ApplySearchAndSort(IncludeAll().Where(a => a.CreatedByTeacherId == teacherId), pagination);
        var page = await PaginatedList<Assignment>.CreateAsync(query, pagination.Page, pagination.PageSize, cancellationToken);
        return page.Map(a => a.ToDto());
    }

    public async Task<PaginatedList<AssignmentDto>> GetPublishedForStudentAsync(Guid studentId, PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var classIds = await _studentClassRepository.GetClassIdsForStudentAsync(studentId, cancellationToken);

        var query = ApplySearchAndSort(
            IncludeAll().Where(a => a.Status == AssignmentStatus.Published && classIds.Contains(a.ClassSubject!.ClassId)),
            pagination);

        var page = await PaginatedList<Assignment>.CreateAsync(query, pagination.Page, pagination.PageSize, cancellationToken);
        return page.Map(a => a.ToDto());
    }

    public async Task<AssignmentDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await IncludeAll().FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Assignment '{id}' was not found.");

        return assignment.ToDto();
    }

    public async Task<AssignmentDto> CreateAsync(Guid teacherId, CreateAssignmentRequest request, CancellationToken cancellationToken = default)
    {
        var classSubject = await _classSubjectRepository.GetByIdAsync(request.ClassSubjectId, cancellationToken)
            ?? throw new KeyNotFoundException("Class/subject assignment not found.");

        if (classSubject.TeacherId != teacherId)
        {
            throw new ForbiddenOperationException("You are not assigned to teach this class/subject.");
        }

        var assignment = new Assignment
        {
            Title = request.Title,
            Description = request.Description,
            DeadlineUtc = request.DeadlineUtc,
            MaxMarks = request.MaxMarks,
            ClassSubjectId = request.ClassSubjectId,
            CreatedByTeacherId = teacherId,
            Status = AssignmentStatus.Draft
        };

        await _assignmentRepository.AddAsync(assignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(assignment.Id, cancellationToken);
    }

    public async Task<AssignmentDto> UpdateAsync(Guid id, UpdateAssignmentRequest request, CancellationToken cancellationToken = default)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Assignment '{id}' was not found.");

        assignment.Title = request.Title;
        assignment.Description = request.Description;
        assignment.DeadlineUtc = request.DeadlineUtc;
        assignment.MaxMarks = request.MaxMarks;

        _assignmentRepository.Update(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(assignment.Id, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Assignment '{id}' was not found.");

        assignment.IsDeleted = true;

        _assignmentRepository.Update(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<AssignmentDto> PublishAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Assignment '{id}' was not found.");

        assignment.Publish();
        _assignmentRepository.Update(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var classSubject = await _classSubjectRepository.GetByIdAsync(assignment.ClassSubjectId, cancellationToken);
        var studentIds = await _studentClassRepository.GetStudentIdsForClassAsync(classSubject!.ClassId, cancellationToken);

        var payload = JsonSerializer.Serialize(new { assignmentId = assignment.Id, title = assignment.Title });

        foreach (var studentId in studentIds)
        {
            await _notificationRepository.AddAsync(new Notification
            {
                UserId = studentId,
                Type = NotificationType.AssignmentPublished,
                Payload = payload
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _realtimeNotifier.NotifyAssignmentPublishedAsync(studentIds, assignment.Id, assignment.Title, cancellationToken);

        return await GetByIdAsync(assignment.Id, cancellationToken);
    }

    public Task<bool> IsOwnedByTeacherAsync(Guid assignmentId, Guid teacherId, CancellationToken cancellationToken = default) =>
        _assignmentRepository.Query()
            .Include(a => a.ClassSubject)
            .AnyAsync(a => a.Id == assignmentId && (a.CreatedByTeacherId == teacherId || a.ClassSubject!.TeacherId == teacherId), cancellationToken);

    private IQueryable<Assignment> IncludeAll() =>
        _assignmentRepository.Query()
            .Include(a => a.ClassSubject!).ThenInclude(cs => cs.Class)
            .Include(a => a.ClassSubject!).ThenInclude(cs => cs.Subject)
            .Include(a => a.CreatedByTeacher);

    private static IQueryable<Assignment> ApplySearchAndSort(IQueryable<Assignment> query, PaginationParams pagination)
    {
        if (!string.IsNullOrWhiteSpace(pagination.Search))
        {
            query = query.Where(a => a.Title.Contains(pagination.Search));
        }

        if (string.IsNullOrWhiteSpace(pagination.SortBy))
        {
            return query.OrderByDescending(a => a.CreatedAt);
        }

        return pagination.SortBy.Trim().ToLowerInvariant() switch
        {
            "title" => pagination.IsDescending ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
            "deadline" or "deadlineutc" => pagination.IsDescending ? query.OrderByDescending(a => a.DeadlineUtc) : query.OrderBy(a => a.DeadlineUtc),
            "maxmarks" => pagination.IsDescending ? query.OrderByDescending(a => a.MaxMarks) : query.OrderBy(a => a.MaxMarks),
            "status" => pagination.IsDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
            _ => pagination.IsDescending ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt)
        };
    }
}
