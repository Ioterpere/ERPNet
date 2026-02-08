using ERPNet.Domain.Enums;

namespace ERPNet.Application.Auth;

public record UsuarioContext(int Id, string Email, int EmpleadoId, int SeccionId, List<PermisoUsuario> Permisos);

public record PermisoUsuario(string Codigo, bool CanCreate, bool CanEdit, bool CanDelete, Alcance Alcance);
