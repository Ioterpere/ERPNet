using FluentValidation;

namespace ERPNet.Application.Common.DTOs.Validators;

public class MoverMenuRequestValidator : AbstractValidator<MoverMenuRequest>
{
    public MoverMenuRequestValidator()
    {
        RuleFor(x => x.Orden)
            .GreaterThan(0).WithMessage("El orden debe ser mayor que 0.");
    }
}
