namespace ERPNet.Contracts;

public record ListaPaginada<T>
{
    public required List<T> Items { get; init; }
    public required int Pagina { get; init; }
    public required int PorPagina { get; init; }
    public required int TotalRegistros { get; init; }
    public int TotalPaginas => (int)Math.Ceiling((double)TotalRegistros / PorPagina);

    public static ListaPaginada<T> Crear(List<T> items, int totalRegistros, int pagina, int porPagina)
        => new()
        {
            Items = items,
            Pagina = pagina,
            PorPagina = porPagina,
            TotalRegistros = totalRegistros,
        };
}
