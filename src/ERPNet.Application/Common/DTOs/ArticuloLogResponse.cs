namespace ERPNet.Application.Common.DTOs;

public record ArticuloLogResponse
{
    public long Id { get; init; }
    public int ArticuloId { get; init; }
    public int UsuarioId { get; init; }
    public string? UsuarioNombre { get; init; }
    public DateOnly Fecha { get; init; }
    public required string Nota { get; init; }
    public decimal? StockAnterior { get; init; }
    public decimal? StockNuevo { get; init; }
    public DateTime CreatedAt { get; init; }
}
