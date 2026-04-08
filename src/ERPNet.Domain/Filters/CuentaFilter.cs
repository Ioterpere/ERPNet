namespace ERPNet.Domain.Filters;

public class CuentaFilter : PaginacionFilter
{
    public string? Codigo { get; init; }
    public string? Descripcion { get; init; }
    public string? Nif { get; init; }
    public bool? ConNif { get; init; }
    public bool? ConDescripcionSii { get; init; }
    public bool? ConSaldo { get; init; }
    public bool? SoloConApuntes { get; init; }

    // Filtros de extracto (filtran cuentas que tengan apuntes que cumplan)
    public int? TipoDiarioId { get; init; }
    public int? CentroCosteId { get; init; }
    public DateOnly? Desde { get; init; }
    public DateOnly? Hasta { get; init; }
    public bool? EsDefinitivo { get; init; }
    public bool? Punteado { get; init; }
}
