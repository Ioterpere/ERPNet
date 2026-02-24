namespace ERPNet.Application.Common.DTOs;

public record EmpleadoResponse
{
    public int Id { get; init; }
    public required string Nombre { get; init; }
    public required string Apellidos { get; init; }
    public required string Dni { get; init; }
    public bool Activo { get; init; }
    public int SeccionId { get; init; }
    public string? SeccionNombre { get; init; }
    public int? EncargadoId { get; init; }
    public Guid? FotoId { get; init; }
}
