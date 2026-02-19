using ERPNet.Contracts.DTOs;
using ERPNet.Domain.Repositories;
using FluentValidation;

namespace ERPNet.Application.Common.DTOs.Validators;

public class CreateEmpleadoRequestValidator : AbstractValidator<CreateEmpleadoRequest>
{
    public CreateEmpleadoRequestValidator(IEmpleadoRepository empleadoRepository)
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede tener más de 100 caracteres.");

        RuleFor(x => x.Apellidos)
            .NotEmpty().WithMessage("Los apellidos son obligatorios.")
            .MaximumLength(200).WithMessage("Los apellidos no pueden tener más de 200 caracteres.");

        RuleFor(x => x.Dni)
            .NotEmpty().WithMessage("El DNI es obligatorio.")
            .MaximumLength(9).WithMessage("El DNI no puede tener más de 9 caracteres.")
            .Matches(@"^\d{8}[A-Za-z]$").WithMessage("El formato del DNI no es válido. Debe ser 8 dígitos seguidos de una letra.")
            .MustAsync(async (dni, ct) => !await empleadoRepository.ExisteDniAsync(dni))
                .WithMessage("Ya existe un empleado con ese DNI.");

        RuleFor(x => x.SeccionId)
            .GreaterThan(0).WithMessage("El SeccionId debe ser mayor que 0.");

        RuleFor(x => x.EncargadoId)
            .GreaterThan(0).WithMessage("El EncargadoId debe ser mayor que 0.")
            .When(x => x.EncargadoId.HasValue);
    }
}
