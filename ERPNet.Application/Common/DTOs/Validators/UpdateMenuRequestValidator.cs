using FluentValidation;

namespace ERPNet.Application.Common.DTOs.Validators;

public class UpdateMenuRequestValidator : AbstractValidator<UpdateMenuRequest>
{
    public UpdateMenuRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede tener más de 100 caracteres.");

        RuleFor(x => x.Orden)
            .GreaterThan(0).WithMessage("El orden debe ser mayor que 0.");
    }
}
