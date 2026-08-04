namespace AssignmentHub.Application.Assignments.Dtos;

public record AssignmentDto(
    Guid Id,
    string Title,
    string Description,
    DateTime DeadlineUtc,
    int MaxMarks,
    string Status,
    Guid ClassSubjectId,
    string ClassName,
    string SubjectName,
    Guid CreatedByTeacherId,
    string CreatedByTeacherName,
    DateTime CreatedAt);
