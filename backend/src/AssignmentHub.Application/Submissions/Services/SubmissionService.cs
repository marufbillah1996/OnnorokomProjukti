using AssignmentHub.Application.Assignments.Interfaces;
using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Application.Common.Models;
using AssignmentHub.Application.Notifications.Interfaces;
using AssignmentHub.Application.Submissions.Dtos;
using AssignmentHub.Application.Submissions.Interfaces;
using AssignmentHub.Application.Submissions.Mappings;
using AssignmentHub.Domain.Entities;
using AssignmentHub.Domain.Enums;
using AssignmentHub.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AssignmentHub.Application.Submissions.Services;

public class SubmissionService : ISubmissionService
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IGradeAuditLogRepository _gradeAuditLogRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IRealtimeNotifier _realtimeNotifier;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SubmissionService(
        ISubmissionRepository submissionRepository,
        IGradeAuditLogRepository gradeAuditLogRepository,
        IAssignmentRepository assignmentRepository,
        INotificationRepository notificationRepository,
        IRealtimeNotifier realtimeNotifier,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _submissionRepository = submissionRepository;
        _gradeAuditLogRepository = gradeAuditLogRepository;
        _assignmentRepository = assignmentRepository;
        _notificationRepository = notificationRepository;
        _realtimeNotifier = realtimeNotifier;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    private IQueryable<Submission> IncludeAll() =>
        _submissionRepository.Query()
            .Include(s => s.Assignment)
            .Include(s => s.Student);

    public async Task<SubmissionDto> SubmitAsync(Guid assignmentId, Guid studentId, SubmitAnswerRequest request, CancellationToken cancellationToken = default)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId, cancellationToken)
            ?? throw new KeyNotFoundException("Assignment not found.");

        var now = _dateTimeProvider.UtcNow;

        if (!assignment.AcceptsSubmissions(now))
        {
            if (assignment.IsPastDeadline(now))
            {
                throw new DeadlinePassedException(assignmentId, assignment.DeadlineUtc);
            }

            throw new InvalidOperationException("This assignment is not open for submissions.");
        }

        if (await _submissionRepository.GetByAssignmentAndStudentAsync(assignmentId, studentId, cancellationToken) is not null)
        {
            throw new DuplicateSubmissionException(assignmentId, studentId);
        }

        var submission = new Submission
        {
            AssignmentId = assignmentId,
            StudentId = studentId,
            Status = SubmissionStatus.Pending
        };
        submission.UpsertContent(request.Content, now);

        await _submissionRepository.AddAsync(submission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(submission.Id, cancellationToken);
    }

    public async Task<SubmissionDto> UpdateAsync(Guid submissionId, Guid studentId, SubmitAnswerRequest request, CancellationToken cancellationToken = default)
    {
        var submission = await _submissionRepository.GetByIdAsync(submissionId, cancellationToken)
            ?? throw new KeyNotFoundException("Submission not found.");

        if (submission.StudentId != studentId)
        {
            throw new ForbiddenOperationException("You can only update your own submission.");
        }

        if (!submission.CanBeModifiedByStudent)
        {
            throw new InvalidOperationException("This submission has already been graded and can no longer be edited.");
        }

        var assignment = await _assignmentRepository.GetByIdAsync(submission.AssignmentId, cancellationToken)
            ?? throw new KeyNotFoundException("Assignment not found.");

        var now = _dateTimeProvider.UtcNow;

        if (assignment.IsPastDeadline(now))
        {
            throw new DeadlinePassedException(submission.AssignmentId, assignment.DeadlineUtc);
        }

        submission.UpsertContent(request.Content, now);

        // Resubmission after a teacher return goes back to Pending for re-review.
        if (submission.Status == SubmissionStatus.Returned)
        {
            submission.Status = SubmissionStatus.Pending;
        }

        _submissionRepository.Update(submission);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(submission.Id, cancellationToken);
    }

    public async Task<PaginatedList<SubmissionDto>> GetForAssignmentAsync(Guid assignmentId, PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = IncludeAll()
            .Where(s => s.AssignmentId == assignmentId)
            .OrderByDescending(s => s.SubmittedAt);

        var page = await PaginatedList<Submission>.CreateAsync(query, pagination.Page, pagination.PageSize, cancellationToken);

        return page.Map(s => s.ToDto());
    }

    public async Task<PaginatedList<SubmissionDto>> GetForStudentAsync(Guid studentId, PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = IncludeAll()
            .Where(s => s.StudentId == studentId)
            .OrderByDescending(s => s.SubmittedAt);

        var page = await PaginatedList<Submission>.CreateAsync(query, pagination.Page, pagination.PageSize, cancellationToken);

        return page.Map(s => s.ToDto());
    }

    public async Task<PaginatedList<SubmissionDto>> GetAllForAdminAsync(PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = IncludeAll()
            .OrderByDescending(s => s.SubmittedAt);

        var page = await PaginatedList<Submission>.CreateAsync(query, pagination.Page, pagination.PageSize, cancellationToken);

        return page.Map(s => s.ToDto());
    }

    public async Task<SubmissionDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var submission = await IncludeAll().FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Submission not found.");

        return submission.ToDto();
    }

    public async Task<SubmissionDto> GradeAsync(Guid submissionId, Guid teacherId, GradeSubmissionRequest request, CancellationToken cancellationToken = default)
    {
        var submission = await _submissionRepository.GetByIdAsync(submissionId, cancellationToken)
            ?? throw new KeyNotFoundException("Submission not found.");

        if (!await IsGradableByTeacherAsync(submissionId, teacherId, cancellationToken))
        {
            throw new ForbiddenOperationException("You are not authorized to grade this submission.");
        }

        var assignment = await _assignmentRepository.GetByIdAsync(submission.AssignmentId, cancellationToken)
            ?? throw new KeyNotFoundException("Assignment not found.");

        var previousStatus = submission.Status;
        var previousMarks = submission.MarksAwarded;

        submission.Grade(request.MarksAwarded, request.Feedback, assignment.MaxMarks);

        _submissionRepository.Update(submission);

        await _gradeAuditLogRepository.AddAsync(new GradeAuditLog
        {
            SubmissionId = submission.Id,
            GradedByTeacherId = teacherId,
            PreviousStatus = previousStatus,
            NewStatus = submission.Status,
            PreviousMarks = previousMarks,
            NewMarks = submission.MarksAwarded,
            GradedAt = _dateTimeProvider.UtcNow
        }, cancellationToken);

        await _notificationRepository.AddAsync(new Notification
        {
            UserId = submission.StudentId,
            Type = NotificationType.SubmissionGraded,
            Payload = System.Text.Json.JsonSerializer.Serialize(new { submissionId = submission.Id, marks = submission.MarksAwarded })
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _realtimeNotifier.NotifySubmissionGradedAsync(submission.StudentId, submission.Id, submission.MarksAwarded, cancellationToken);

        return await GetByIdAsync(submission.Id, cancellationToken);
    }

    public async Task<SubmissionDto> ChangeStatusAsync(Guid submissionId, Guid teacherId, ChangeSubmissionStatusRequest request, CancellationToken cancellationToken = default)
    {
        var submission = await _submissionRepository.GetByIdAsync(submissionId, cancellationToken)
            ?? throw new KeyNotFoundException("Submission not found.");

        if (!await IsGradableByTeacherAsync(submissionId, teacherId, cancellationToken))
        {
            throw new ForbiddenOperationException("You are not authorized to change this submission's status.");
        }

        if (!Enum.TryParse<SubmissionStatus>(request.Status, true, out var target) || target == SubmissionStatus.Graded)
        {
            throw new InvalidOperationException("Status must be either 'Pending' or 'Returned' — use the grade endpoint to mark a submission as Graded.");
        }

        var previousStatus = submission.Status;

        if (target == SubmissionStatus.Returned)
        {
            submission.ReturnToStudent(submission.Feedback);
        }
        else
        {
            submission.Status = SubmissionStatus.Pending;
        }

        _submissionRepository.Update(submission);

        await _gradeAuditLogRepository.AddAsync(new GradeAuditLog
        {
            SubmissionId = submission.Id,
            GradedByTeacherId = teacherId,
            PreviousStatus = previousStatus,
            NewStatus = submission.Status,
            PreviousMarks = submission.MarksAwarded,
            NewMarks = submission.MarksAwarded,
            GradedAt = _dateTimeProvider.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(submission.Id, cancellationToken);
    }

    public Task<bool> IsGradableByTeacherAsync(Guid submissionId, Guid teacherId, CancellationToken cancellationToken = default) =>
        _submissionRepository.Query()
            .Include(s => s.Assignment!)
            .ThenInclude(a => a.ClassSubject)
            .AnyAsync(s => s.Id == submissionId
                && (s.Assignment!.CreatedByTeacherId == teacherId || s.Assignment!.ClassSubject!.TeacherId == teacherId),
                cancellationToken);
}
