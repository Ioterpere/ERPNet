using ERPNet.Domain.Enums;

namespace ERPNet.Application.Auth;

public record UsuarioContext
{
    public int Id { get; init; }
    public required string Email { get; init; }
    public int EmpleadoId { get; init; }
    public int SeccionId { get; init; }
    public int? EmpresaId { get; init; }
    public List<int> EmpresaIds { get; init; } = [];
    public List<PermisoUsuario> Permisos { get; init; } = [];
    public List<int> RolIds { get; init; } = [];
    public bool RequiereCambioContrasena { get; init; }
}

public record PermisoUsuario
{
    public RecursoCodigo Codigo { get; init; }
    public bool CanCreate { get; init; }
    public bool CanEdit { get; init; }
    public bool CanDelete { get; init; }
    public Alcance Alcance { get; init; }
}
