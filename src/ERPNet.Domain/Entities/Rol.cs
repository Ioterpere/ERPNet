using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class Rol : BaseEntity
{
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }

    public ICollection<RolUsuario> RolesUsuarios { get; set; } = [];
    public ICollection<PermisoRolRecurso> PermisosRolRecurso { get; set; } = [];
    public ICollection<MenuRol> MenusRoles { get; set; } = [];
}
