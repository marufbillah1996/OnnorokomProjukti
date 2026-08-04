using AssignmentHub.Application.Academics.Dtos;
using FluentValidation;

namespace AssignmentHub.Application.Academics.Validators;

public class UpdateSubjectRequestValidator : AbstractValidator<UpdateSubjectRequest>
{
    public UpdateSubjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(20);
    }
}
