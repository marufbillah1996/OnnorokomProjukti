namespace AssignmentHub.Application.Assignments.Dtos;

public record CreateAssignmentRequest(
    string Title,
    string Description,
    DateTime DeadlineUtc,
    int MaxMarks,
    Guid ClassSubjectId);
