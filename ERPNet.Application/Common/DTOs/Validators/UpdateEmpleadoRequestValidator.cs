using ERPNet.Contracts.DTOs;
using FluentValidation;

namespace ERPNet.Application.Common.DTOs.Validators;

public class UpdateEmpleadoRequestValidator : AbstractValidator<UpdateEmpleadoRequest>
{
    public UpdateEmpleadoRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre no puede estar vacío.")
            .MaximumLength(100).WithMessage("El nombre no puede tener más de 100 caracteres.")
            .When(x => x.Nombre is not null);

        RuleFor(x => x.Apellidos)
            .NotEmpty().WithMessage("Los apellidos no pueden estar vacíos.")
            .MaximumLength(200).WithMessage("Los apellidos no pueden tener más de 200 caracteres.")
            .When(x => x.Apellidos is not null);

        RuleFor(x => x.Dni)
            .NotEmpty().WithMessage("El DNI no puede estar vacío.")
            .MaximumLength(9).WithMessage("El DNI no puede tener más de 9 caracteres.")
            .Matches(@"^\d{8}[A-Za-z]$").WithMessage("El formato del DNI no es válido. Debe ser 8 dígitos seguidos de una letra.")
            .When(x => x.Dni is not null);

        RuleFor(x => x.SeccionId)
            .GreaterThan(0).WithMessage("El SeccionId debe ser mayor que 0.")
            .When(x => x.SeccionId.HasValue);

        RuleFor(x => x.EncargadoId)
            .GreaterThan(0).WithMessage("El EncargadoId debe ser mayor que 0.")
            .When(x => x.EncargadoId.HasValue);

        RuleFor(x => x)
            .Must(x => x.Nombre is not null || x.Apellidos is not null || x.Dni is not null
                || x.Activo.HasValue || x.SeccionId.HasValue || x.EncargadoId.HasValue)
            .WithMessage("Debe informar al menos un campo.");
    }
}
