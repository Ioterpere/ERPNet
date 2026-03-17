namespace ERPNet.Application.Common.DTOs;

public record UpdateMaquinariaRequest
{
    public string? Nombre { get; init; }
    public string? Codigo { get; init; }
    public string? Ubicacion { get; init; }
    public bool? Activa { get; init; }
    public int? SeccionId { get; init; }
}
