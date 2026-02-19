using ERPNet.Domain.Filters;

namespace ERPNet.Application.Common.DTOs;

public record ListaPaginada<T>
{
    public required List<T> Items { get; init; }
    public required int Pagina { get; init; }
    public required int PorPagina { get; init; }
    public required int TotalRegistros { get; init; }
    public int TotalPaginas => (int)Math.Ceiling((double)TotalRegistros / PorPagina);

    public static ListaPaginada<T> Crear(List<T> items, int totalRegistros, PaginacionFilter filtro)
        => new()
        {
            Items = items,
            Pagina = filtro.Pagina,
            PorPagina = filtro.PorPagina,
            TotalRegistros = totalRegistros,
        };
}
