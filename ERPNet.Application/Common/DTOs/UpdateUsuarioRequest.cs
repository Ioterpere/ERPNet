namespace ERPNet.Application.Common.DTOs;

public record UpdateUsuarioRequest
{
    public string? Email { get; init; }
    public int? EmpleadoId { get; init; }
    public bool? Activo { get; init; }
    public DateTime? CaducidadContrasena { get; init; }
}
