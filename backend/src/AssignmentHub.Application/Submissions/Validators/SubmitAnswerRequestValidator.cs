using AssignmentHub.Application.Submissions.Dtos;
using FluentValidation;

namespace AssignmentHub.Application.Submissions.Validators;

public class SubmitAnswerRequestValidator : AbstractValidator<SubmitAnswerRequest>
{
    public SubmitAnswerRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(20000);
    }
}
