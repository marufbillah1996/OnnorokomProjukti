using AssignmentHub.Application.Settings.Dtos;
using FluentValidation;

namespace AssignmentHub.Application.Settings.Validators;

public class UpsertSettingRequestValidator : AbstractValidator<UpsertSettingRequest>
{
    public UpsertSettingRequestValidator()
    {
        RuleFor(x => x.SettingKey)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.SettingValue)
            .NotEmpty()
            .MaximumLength(2000);
    }
}
