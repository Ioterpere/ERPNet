using FluentValidation;

namespace ERPNet.Application.Common.DTOs.Validators;

public class UpdateArticuloRequestValidator : AbstractValidator<UpdateArticuloRequest>
{
    public UpdateArticuloRequestValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código no puede estar vacío.")
            .MaximumLength(50).WithMessage("El código no puede tener más de 50 caracteres.")
            .When(x => x.Codigo is not null);

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción no puede estar vacía.")
            .MaximumLength(300).WithMessage("La descripción no puede tener más de 300 caracteres.")
            .When(x => x.Descripcion is not null);

        RuleFor(x => x.UnidadMedida)
            .MaximumLength(20).WithMessage("La unidad de medida no puede tener más de 20 caracteres.")
            .When(x => x.UnidadMedida is not null);

        RuleFor(x => x.PrecioCompra)
            .GreaterThanOrEqualTo(0).WithMessage("El precio de compra debe ser mayor o igual a 0.")
            .When(x => x.PrecioCompra.HasValue);

        RuleFor(x => x.PrecioVenta)
            .GreaterThanOrEqualTo(0).WithMessage("El precio de venta debe ser mayor o igual a 0.")
            .When(x => x.PrecioVenta.HasValue);
    }
}
