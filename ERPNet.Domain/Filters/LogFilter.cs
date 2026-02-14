namespace ERPNet.Domain.Filters;

public class LogFilter : PaginacionFilter
{
    public string? Entidad { get; init; }
    public string? EntidadId { get; init; }
    public int? UsuarioId { get; init; }
    public string? Accion { get; init; }
    public DateTime? Desde { get; init; }
    public DateTime? Hasta { get; init; }
}
