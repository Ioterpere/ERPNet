namespace ERPNet.Domain.Filters;

public class ExtractoFilter
{
    public int? TipoDiarioId { get; init; }
    public int? CentroCosteId { get; init; }
    public DateOnly? Desde { get; init; }
    public DateOnly? Hasta { get; init; }
    public bool? EsDefinitivo { get; init; }
    public bool? Punteado { get; init; }   // null=todos, true=punteados, false=sin puntear
}
