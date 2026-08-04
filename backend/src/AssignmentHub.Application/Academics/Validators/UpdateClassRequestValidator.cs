using AssignmentHub.Application.Academics.Dtos;
using FluentValidation;

namespace AssignmentHub.Application.Academics.Validators;

public class UpdateClassRequestValidator : AbstractValidator<UpdateClassRequest>
{
    public UpdateClassRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}
