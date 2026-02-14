using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class MenuRol : ISoftDeletable
{
    public int MenuId { get; set; }
    public Menu Menu { get; set; } = null!;

    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;
    public bool IsDeleted { get; set; }
}
