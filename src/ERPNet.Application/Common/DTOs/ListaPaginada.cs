namespace ERPNet.Application.Common.DTOs;

public record ListaPaginada<T>
{
    public required List<T> Items { get; init; }
    public required int TotalRegistros { get; init; }

    public static ListaPaginada<T> Crear(List<T> items, int totalRegistros)
        => new() { Items = items, TotalRegistros = totalRegistros };
}
