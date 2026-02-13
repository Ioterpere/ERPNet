using FluentValidation;

namespace ERPNet.Application.DTOs.Validators;

public class UpdateUsuarioRequestValidator : AbstractValidator<UpdateUsuarioRequest>
{
    public UpdateUsuarioRequestValidator()
    {
        RuleFor(x => x.Email)
            .MaximumLength(256).WithMessage("El email no puede tener más de 256 caracteres.")
            .EmailAddress().WithMessage("El formato del email no es válido.")
            .When(x => x.Email is not null);

        RuleFor(x => x.EmpleadoId)
            .GreaterThan(0).WithMessage("El EmpleadoId debe ser mayor que 0.")
            .When(x => x.EmpleadoId.HasValue);

        RuleFor(x => x)
            .Must(x => x.Email is not null || x.EmpleadoId.HasValue || x.Activo.HasValue)
            .WithMessage("Debe informar al menos un campo.");
    }
}
