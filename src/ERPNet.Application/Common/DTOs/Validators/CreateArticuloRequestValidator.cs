using FluentValidation;

namespace ERPNet.Application.Common.DTOs.Validators;

public class CreateArticuloRequestValidator : AbstractValidator<CreateArticuloRequest>
{
    public CreateArticuloRequestValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código es obligatorio.")
            .MaximumLength(50).WithMessage("El código no puede tener más de 50 caracteres.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(300).WithMessage("La descripción no puede tener más de 300 caracteres.");

        RuleFor(x => x.UnidadMedida)
            .MaximumLength(20).WithMessage("La unidad de medida no puede tener más de 20 caracteres.")
            .When(x => x.UnidadMedida is not null);
    }
}
