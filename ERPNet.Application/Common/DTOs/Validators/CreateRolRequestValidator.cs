using ERPNet.Domain.Repositories;
using FluentValidation;

namespace ERPNet.Application.Common.DTOs.Validators;

public class CreateRolRequestValidator : AbstractValidator<CreateRolRequest>
{
    public CreateRolRequestValidator(IRolRepository rolRepository)
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede tener más de 100 caracteres.")
            .MustAsync(async (nombre, ct) => !await rolRepository.ExisteNombreAsync(nombre))
                .WithMessage("Ya existe un rol con ese nombre.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(500).WithMessage("La descripción no puede tener más de 500 caracteres.")
            .When(x => x.Descripcion is not null);
    }
}
