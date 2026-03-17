using System.ComponentModel.DataAnnotations;

namespace ERPNet.Domain.Filters;

public class PaginacionFilter
{
    [Range(0, int.MaxValue)]
    public int Pagina { get; init; } = 0;

    [Range(1, 200)]
    public int PorPagina { get; init; } = 50;

    public string? Busqueda { get; init; }

    public string? OrdenarPor { get; init; }

    public bool OrdenDesc { get; init; }
}
