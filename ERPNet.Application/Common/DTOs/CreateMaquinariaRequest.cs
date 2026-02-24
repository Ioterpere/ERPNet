namespace ERPNet.Application.Common.DTOs;

public record CreateMaquinariaRequest
{
    public required string Nombre { get; init; }
    public required string Codigo { get; init; }
    public string? Ubicacion { get; init; }
    public int? SeccionId { get; init; }
}
