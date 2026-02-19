using ERPNet.Contracts.DTOs;
using FluentValidation;

namespace ERPNet.Application.Common.DTOs.Validators;

public class ResetearContrasenaRequestValidator : AbstractValidator<ResetearContrasenaRequest>
{
    public ResetearContrasenaRequestValidator()
    {
        RuleFor(x => x.NuevaContrasena)
            .NotEmpty().WithMessage("La nueva contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una mayúscula.")
            .Matches("[a-z]").WithMessage("La contraseña debe contener al menos una minúscula.")
            .Matches("[0-9]").WithMessage("La contraseña debe contener al menos un dígito.");

        RuleFor(x => x.ConfirmarContrasena)
            .Equal(x => x.NuevaContrasena).WithMessage("La confirmación no coincide con la nueva contraseña.");
    }
}
