using ERPNet.Domain.Enums;

namespace ERPNet.Application.Auth.DTOs;

public record UsuarioContext
{
    public int Id { get; init; }
    public required string Email { get; init; }
    public int EmpleadoId { get; init; }
    public int SeccionId { get; init; }
    public int? EmpresaId { get; init; }
    public List<int> EmpresaIds { get; init; } = [];
    public List<PermisoResponse> Permisos { get; init; } = [];
    public List<int> RolIds { get; init; } = [];
    public bool RequiereCambioContrasena { get; init; }
    public Plataforma? Plataforma { get; init; }
}
