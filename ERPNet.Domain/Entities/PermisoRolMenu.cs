namespace ERPNet.Domain.Entities;

public class PermisoRolMenu
{
    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;

    public int MenuId { get; set; }
    public Menu Menu { get; set; } = null!;

    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}
