using AssignmentHub.Application.Submissions.Dtos;
using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Application.Submissions.Mappings;

/// <summary>
/// Assumes the caller has eager-loaded the Assignment and Student navigations (e.g. via
/// .Include(s => s.Assignment).Include(s => s.Student) on the repository's Query()) before
/// calling ToDto — this mapper does not lazy-load.
/// </summary>
public static class SubmissionMappingExtensions
{
    public static SubmissionDto ToDto(this Submission s) => new(
        s.Id,
        s.AssignmentId,
        s.Assignment!.Title,
        s.StudentId,
        s.Student!.Name,
        s.Content,
        s.SubmittedAt,
        s.Status.ToString(),
        s.MarksAwarded,
        s.Assignment!.MaxMarks,
        s.Feedback);
}
