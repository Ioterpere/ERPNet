namespace ERPNet.Application.Common.DTOs;

public record CreateEmpleadoRequest
{
    public required string Nombre { get; init; }
    public required string Apellidos { get; init; }
    public required string Dni { get; init; }
    public int SeccionId { get; init; }
    public int? EncargadoId { get; init; }
}
