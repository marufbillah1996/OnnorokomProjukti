namespace AssignmentHub.Application.Academics.Dtos;

public record ClassSubjectDto(
    Guid Id,
    Guid ClassId,
    string ClassName,
    Guid SubjectId,
    string SubjectName,
    Guid TeacherId,
    string TeacherName);
