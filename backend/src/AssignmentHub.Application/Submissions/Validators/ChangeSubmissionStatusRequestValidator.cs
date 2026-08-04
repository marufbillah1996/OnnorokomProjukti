using AssignmentHub.Application.Submissions.Dtos;
using FluentValidation;

namespace AssignmentHub.Application.Submissions.Validators;

public class ChangeSubmissionStatusRequestValidator : AbstractValidator<ChangeSubmissionStatusRequest>
{
    public ChangeSubmissionStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => s.Equals("Pending", StringComparison.OrdinalIgnoreCase) || s.Equals("Returned", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Status must be 'Pending' or 'Returned'.");
    }
}
