using ERPNet.Contracts.DTOs;
using ERPNet.Domain.Repositories;
using FluentValidation;

namespace ERPNet.Application.Common.DTOs.Validators;

public class CreateMaquinariaRequestValidator : AbstractValidator<CreateMaquinariaRequest>
{
    public CreateMaquinariaRequestValidator(IMaquinariaRepository maquinariaRepository)
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede tener más de 200 caracteres.");

        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código es obligatorio.")
            .MaximumLength(50).WithMessage("El código no puede tener más de 50 caracteres.")
            .MustAsync(async (codigo, ct) => !await maquinariaRepository.ExisteCodigoAsync(codigo))
                .WithMessage("Ya existe una máquina con ese código.");

        RuleFor(x => x.Ubicacion)
            .MaximumLength(200).WithMessage("La ubicación no puede tener más de 200 caracteres.")
            .When(x => x.Ubicacion is not null);

        RuleFor(x => x.SeccionId)
            .GreaterThan(0).WithMessage("El SeccionId debe ser mayor que 0.")
            .When(x => x.SeccionId.HasValue);
    }
}
