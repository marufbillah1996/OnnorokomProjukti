using AssignmentHub.Application.Academics.Dtos;
using FluentValidation;

namespace AssignmentHub.Application.Academics.Validators;

public class CreateClassRequestValidator : AbstractValidator<CreateClassRequest>
{
    public CreateClassRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}
