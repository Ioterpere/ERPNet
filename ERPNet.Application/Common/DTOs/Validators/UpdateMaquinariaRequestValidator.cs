using FluentValidation;

namespace ERPNet.Application.Common.DTOs.Validators;

public class UpdateMaquinariaRequestValidator : AbstractValidator<UpdateMaquinariaRequest>
{
    public UpdateMaquinariaRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre no puede estar vacío.")
            .MaximumLength(200).WithMessage("El nombre no puede tener más de 200 caracteres.")
            .When(x => x.Nombre is not null);

        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código no puede estar vacío.")
            .MaximumLength(50).WithMessage("El código no puede tener más de 50 caracteres.")
            .When(x => x.Codigo is not null);

        RuleFor(x => x.Ubicacion)
            .MaximumLength(200).WithMessage("La ubicación no puede tener más de 200 caracteres.")
            .When(x => x.Ubicacion is not null);

        RuleFor(x => x.SeccionId)
            .GreaterThan(0).WithMessage("El SeccionId debe ser mayor que 0.")
            .When(x => x.SeccionId.HasValue);

        RuleFor(x => x)
            .Must(x => x.Nombre is not null || x.Codigo is not null || x.Ubicacion is not null
                || x.Activa.HasValue || x.SeccionId.HasValue)
            .WithMessage("Debe informar al menos un campo.");
    }
}
