using FluentValidation;

namespace ERPNet.Application.DTOs.Validators;

public class CreateMenuRequestValidator : AbstractValidator<CreateMenuRequest>
{
    public CreateMenuRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede tener más de 100 caracteres.");

        RuleFor(x => x.Orden)
            .GreaterThan(0).WithMessage("El orden debe ser mayor que 0.");

        RuleFor(x => x.Plataforma)
            .IsInEnum().WithMessage("La plataforma no es válida.");
    }
}
