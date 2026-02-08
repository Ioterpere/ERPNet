using ERPNet.Domain.Enums;

namespace ERPNet.Domain.Entities;

public class PermisoRolRecurso
{
    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;

    public int RecursoId { get; set; }
    public Recurso Recurso { get; set; } = null!;

    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public Alcance Alcance { get; set; }
}
