namespace ERPNet.Application.Common.DTOs;

public class SetPermisosRolRequest
{
    public List<PermisoRolRecursoDto> Permisos { get; set; } = [];
}

public class PermisoRolRecursoDto
{
    public int RecursoId { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public int Alcance { get; set; }
}
