namespace ERPNet.Application.Common.DTOs;

public record UpdateEmpleadoRequest
{
    public string? Nombre { get; init; }
    public string? Apellidos { get; init; }
    public string? Dni { get; init; }
    public bool? Activo { get; init; }
    public int? SeccionId { get; init; }
    public int? EncargadoId { get; init; }
}
