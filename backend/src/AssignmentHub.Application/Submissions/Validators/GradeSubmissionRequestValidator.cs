using AssignmentHub.Application.Submissions.Dtos;
using FluentValidation;

namespace AssignmentHub.Application.Submissions.Validators;

public class GradeSubmissionRequestValidator : AbstractValidator<GradeSubmissionRequest>
{
    public GradeSubmissionRequestValidator()
    {
        // Upper bound against the assignment's MaxMarks is enforced by the domain method
        // Submission.Grade, not here, since this validator has no DB access to the assignment.
        RuleFor(x => x.MarksAwarded)
            .GreaterThanOrEqualTo(0);
    }
}
