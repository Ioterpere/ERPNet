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

        RuleFor(x => x.CodigoBarras)
            .MaximumLength(13).WithMessage("El código de barras no puede tener más de 13 caracteres.")
            .When(x => x.CodigoBarras is not null);

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción no puede estar vacía.")
            .MaximumLength(300).WithMessage("La descripción no puede tener más de 300 caracteres.")
            .When(x => x.Descripcion is not null);

        RuleFor(x => x.DescripcionVenta)
            .MaximumLength(300).WithMessage("La descripción de venta no puede tener más de 300 caracteres.")
            .When(x => x.DescripcionVenta is not null);

        RuleFor(x => x.UnidadMedida)
            .MaximumLength(20).WithMessage("La unidad de medida no puede tener más de 20 caracteres.")
            .When(x => x.UnidadMedida is not null);

        RuleFor(x => x.UnidadMedida2)
            .MaximumLength(3).WithMessage("La unidad de medida 2 no puede tener más de 3 caracteres.")
            .When(x => x.UnidadMedida2 is not null);

        RuleFor(x => x.PrecioCoste)
            .GreaterThanOrEqualTo(0).WithMessage("El precio de coste debe ser mayor o igual a 0.")
            .When(x => x.PrecioCoste.HasValue);

        RuleFor(x => x.PrecioMedio)
            .GreaterThanOrEqualTo(0).WithMessage("El precio medio debe ser mayor o igual a 0.")
            .When(x => x.PrecioMedio.HasValue);

        RuleFor(x => x.PrecioVenta)
            .GreaterThanOrEqualTo(0).WithMessage("El precio de venta debe ser mayor o igual a 0.")
            .When(x => x.PrecioVenta.HasValue);

        RuleFor(x => x.StockMinimo)
            .GreaterThanOrEqualTo(0).WithMessage("El stock mínimo debe ser mayor o igual a 0.")
            .When(x => x.StockMinimo.HasValue);

        RuleFor(x => x.NivelPedido)
            .GreaterThanOrEqualTo(0).WithMessage("El nivel de pedido debe ser mayor o igual a 0.")
            .When(x => x.NivelPedido.HasValue);

        RuleFor(x => x.NivelReposicion)
            .GreaterThanOrEqualTo(0).WithMessage("El nivel de reposición debe ser mayor o igual a 0.")
            .When(x => x.NivelReposicion.HasValue);

        RuleFor(x => x.UnidadesCaja)
            .GreaterThanOrEqualTo(0).WithMessage("Las unidades por caja deben ser mayor o igual a 0.")
            .When(x => x.UnidadesCaja.HasValue);

        RuleFor(x => x.UnidadesPalet)
            .GreaterThanOrEqualTo(0).WithMessage("Las unidades por palé deben ser mayor o igual a 0.")
            .When(x => x.UnidadesPalet.HasValue);

        RuleFor(x => x.FilasPalet)
            .GreaterThanOrEqualTo(0).WithMessage("Las filas de palé deben ser mayor o igual a 0.")
            .When(x => x.FilasPalet.HasValue);

        RuleFor(x => x.PesoGramo)
            .GreaterThanOrEqualTo(0).WithMessage("El peso debe ser mayor o igual a 0.")
            .When(x => x.PesoGramo.HasValue);

        RuleFor(x => x.LeadTime)
            .GreaterThanOrEqualTo(0).WithMessage("El lead time debe ser mayor o igual a 0.")
            .When(x => x.LeadTime.HasValue);

        RuleFor(x => x.DiasVidaUtil)
            .GreaterThanOrEqualTo(0).WithMessage("Los días de vida útil deben ser mayor o igual a 0.")
            .When(x => x.DiasVidaUtil.HasValue);

        RuleFor(x => x.Depreciacion)
            .InclusiveBetween(0, 100).WithMessage("La depreciación debe estar entre 0 y 100.")
            .When(x => x.Depreciacion.HasValue);

        RuleFor(x => x.ProveedorPrincipal)
            .MaximumLength(200).WithMessage("El proveedor principal no puede tener más de 200 caracteres.")
            .When(x => x.ProveedorPrincipal is not null);

        RuleFor(x => x.Observaciones)
            .MaximumLength(2000).WithMessage("Las observaciones no pueden tener más de 2000 caracteres.")
            .When(x => x.Observaciones is not null);
    }
}
