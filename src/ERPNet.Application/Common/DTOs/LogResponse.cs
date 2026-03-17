namespace ERPNet.Application.Common.DTOs;

public record LogResponse
{
    public long Id { get; init; }
    public int? UsuarioId { get; init; }
    public required string Accion { get; init; }
    public string? Entidad { get; init; }
    public string? EntidadId { get; init; }
    public DateTime Fecha { get; init; }
    public string? Detalle { get; init; }
}
