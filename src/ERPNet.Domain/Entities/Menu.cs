using ERPNet.Domain.Common;
using ERPNet.Domain.Enums;

namespace ERPNet.Domain.Entities;

public class Menu : BaseEntity
{
    public string Nombre { get; set; } = null!;
    public int? MenuPadreId { get; set; }
    public string? Path { get; set; }
    public string? Icon { get; set; }
    public string? Tag { get; set; }
    public int Orden { get; set; }
    public Plataforma Plataforma { get; set; }

    public Menu? MenuPadre { get; set; }
    public ICollection<Menu> SubMenus { get; set; } = [];
    public ICollection<MenuRol> MenusRoles { get; set; } = [];
}
