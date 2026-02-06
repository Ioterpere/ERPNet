using ERPNet.Domain.Common;
using ERPNet.Domain.Enums;

namespace ERPNet.Domain.Entities;

public class Menu : BaseEntity
{
    public string Nombre { get; set; } = null!;
    public int? MenuPadreId { get; set; }
    public string? Path { get; set; }
    public string? IconClass { get; set; }
    public string? CustomClass { get; set; }
    public int Orden { get; set; }
    public Plataforma Plataforma { get; set; }

    public Menu? MenuPadre { get; set; }
    public ICollection<Menu> SubMenus { get; set; } = [];
    public ICollection<PermisoRolMenu> PermisosRolMenu { get; set; } = [];
}
