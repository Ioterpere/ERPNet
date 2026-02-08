using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class Recurso : BaseEntity
{
    public string Codigo { get; set; } = null!;
    public string? Descripcion { get; set; }

    public ICollection<PermisoRolRecurso> PermisosRolRecurso { get; set; } = [];
    public ICollection<Menu> Menus { get; set; } = [];
}
