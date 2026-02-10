using System.ComponentModel.DataAnnotations;

namespace ERPNet.Domain.Filters;

public class LogFilter
{
    public string? Entidad { get; init; }
    public string? EntidadId { get; init; }
    public int? UsuarioId { get; init; }
    public string? Accion { get; init; }
    public DateTime? Desde { get; init; }
    public DateTime? Hasta { get; init; }

    [Range(1, int.MaxValue)]
    public int Pagina { get; init; } = 1;

    [Range(1, int.MaxValue)]
    public int PorPagina { get; init; } = 100;
}
