namespace AssignmentHub.Application.Assignments.Dtos;

public record UpdateAssignmentRequest(
    string Title,
    string Description,
    DateTime DeadlineUtc,
    int MaxMarks);
