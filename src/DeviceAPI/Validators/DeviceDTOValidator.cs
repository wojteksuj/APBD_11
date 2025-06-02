using DeviceAPI.DTO;
using DeviceAPI.DTO;
using FluentValidation;

namespace DeviceAPI.Validators;

public class DeviceDTOValidator : AbstractValidator<DeviceDTO>
{
    public DeviceDTOValidator()
    {
        RuleFor(d => d.Name)
            .NotEmpty().WithMessage("Device name is required.")
            .MaximumLength(150);

        RuleFor(d => d.DeviceTypeName)
            .NotEmpty().WithMessage("Device type name is required.")
            .MaximumLength(100);

        RuleFor(d => d.AdditionalProperties)
            .NotNull().WithMessage("AdditionalProperties is required.");

        RuleFor(d => d.IsEnabled)
            .NotNull().WithMessage("IsEnabled must be provided.");
    }
}