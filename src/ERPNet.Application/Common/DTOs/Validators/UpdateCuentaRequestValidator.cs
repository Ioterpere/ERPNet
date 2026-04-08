using FluentValidation;

namespace ERPNet.Application.Common.DTOs.Validators;

public class UpdateCuentaRequestValidator : AbstractValidator<UpdateCuentaRequest>
{
    public UpdateCuentaRequestValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción no puede estar vacía.")
            .MaximumLength(60).WithMessage("La descripción no puede tener más de 60 caracteres.")
            .When(x => x.Descripcion is not null);

        RuleFor(x => x.DescripcionSII)
            .MaximumLength(60).WithMessage("La descripción SII no puede tener más de 60 caracteres.")
            .When(x => x.DescripcionSII is not null);

        RuleFor(x => x.Nif)
            .MaximumLength(15).WithMessage("El NIF no puede tener más de 15 caracteres.")
            .When(x => x.Nif is not null);
    }
}
