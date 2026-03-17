namespace ERPNet.Domain.Filters;

public class EmpleadoFilter : PaginacionFilter
{
    public string? Nombre { get; init; }
    public string? Apellidos { get; init; }
    public bool? Activo { get; init; }
    public int? SeccionId { get; init; }
    public DateOnly? FechaAltaDesde { get; init; }
    public DateOnly? FechaAltaHasta { get; init; }
}
