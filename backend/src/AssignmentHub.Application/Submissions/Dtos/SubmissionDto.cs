namespace AssignmentHub.Application.Submissions.Dtos;

public record SubmissionDto(
    Guid Id,
    Guid AssignmentId,
    string AssignmentTitle,
    Guid StudentId,
    string StudentName,
    string Content,
    DateTime SubmittedAt,
    string Status,
    int? MarksAwarded,
    int MaxMarks,
    string? Feedback);
