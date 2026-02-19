using FluentValidation;

namespace ERPNet.Application.Common.DTOs.Validators;

public class UpdateRolRequestValidator : AbstractValidator<UpdateRolRequest>
{
    public UpdateRolRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre no puede estar vacío.")
            .MaximumLength(100).WithMessage("El nombre no puede tener más de 100 caracteres.")
            .When(x => x.Nombre is not null);

        RuleFor(x => x.Descripcion)
            .MaximumLength(500).WithMessage("La descripción no puede tener más de 500 caracteres.")
            .When(x => x.Descripcion is not null);

        RuleFor(x => x)
            .Must(x => x.Nombre is not null || x.Descripcion is not null)
            .WithMessage("Debe informar al menos un campo.");
    }
}
