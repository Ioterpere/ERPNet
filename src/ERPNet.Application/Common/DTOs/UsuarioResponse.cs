namespace ERPNet.Application.Common.DTOs;

public record UsuarioResponse
{
    public required int Id { get; init; }
    public required string Email { get; init; }
    public int EmpleadoId { get; init; }
    public string EmpleadoNombre { get; init; } = string.Empty;
    public string EmpleadoApellidos { get; init; } = string.Empty;
    public int SeccionId { get; init; }
    public bool Activo { get; init; }
    public bool RequiereCambioContrasena { get; init; }
    public DateTime? UltimoAcceso { get; init; }
    public DateTime? CaducidadContrasena { get; init; }
    public DateTime UltimoCambioContrasena { get; init; }
    public List<RolResponse> Roles { get; init; } = [];
}