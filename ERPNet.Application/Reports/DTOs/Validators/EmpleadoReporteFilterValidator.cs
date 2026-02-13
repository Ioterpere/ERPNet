using FluentValidation;

namespace ERPNet.Application.Reports.DTOs.Validators;

public class EmpleadoReporteFilterValidator : AbstractValidator<EmpleadoReporteFilter>
{
    public EmpleadoReporteFilterValidator()
    {
        RuleFor(x => x.Formato)
            .IsInEnum().WithMessage("El formato no es válido.");

        RuleFor(x => x.SeccionId)
            .GreaterThan(0).WithMessage("El SeccionId debe ser mayor que 0.")
            .When(x => x.SeccionId.HasValue);
    }
}
