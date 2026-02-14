using System.ComponentModel.DataAnnotations;

namespace ERPNet.Domain.Filters;

public class PaginacionFilter
{
    [Range(1, int.MaxValue)]
    public int Pagina { get; init; } = 1;

    [Range(1, int.MaxValue)]
    public int PorPagina { get; init; } = 50;
}
