using FluentValidation;

namespace ERPNet.Application.Common.DTOs.Validators;

public class CreateCuentaRequestValidator : AbstractValidator<CreateCuentaRequest>
{
    public CreateCuentaRequestValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código es obligatorio.")
            .MaximumLength(9).WithMessage("El código no puede tener más de 9 caracteres.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(60).WithMessage("La descripción no puede tener más de 60 caracteres.");

        RuleFor(x => x.DescripcionSII)
            .MaximumLength(60).WithMessage("La descripción SII no puede tener más de 60 caracteres.")
            .When(x => x.DescripcionSII is not null);

        RuleFor(x => x.Nif)
            .MaximumLength(15).WithMessage("El NIF no puede tener más de 15 caracteres.")
            .When(x => x.Nif is not null);
    }
}
