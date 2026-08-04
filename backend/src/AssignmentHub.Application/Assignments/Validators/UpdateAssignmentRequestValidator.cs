using AssignmentHub.Application.Assignments.Dtos;
using AssignmentHub.Application.Common.Interfaces;
using FluentValidation;

namespace AssignmentHub.Application.Assignments.Validators;

public class UpdateAssignmentRequestValidator : AbstractValidator<UpdateAssignmentRequest>
{
    public UpdateAssignmentRequestValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty();

        RuleFor(x => x.DeadlineUtc)
            .Must(d => d > dateTimeProvider.UtcNow)
            .WithMessage("Deadline must be in the future.");

        RuleFor(x => x.MaxMarks)
            .GreaterThan(0);
    }
}
