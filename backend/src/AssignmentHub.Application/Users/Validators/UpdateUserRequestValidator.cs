using FluentValidation;
using AssignmentHub.Application.Users.Dtos;

namespace AssignmentHub.Application.Users.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    private static readonly string[] ValidRoles = { "Admin", "Teacher", "Student" };

    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => ValidRoles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase)))
            .WithMessage("Role must be one of: Admin, Teacher, Student.");
    }
}
