namespace ERPNet.Application.Common.DTOs;

public record SetPermisosRolRequest
{
    public List<PermisoRolRecursoDto> Permisos { get; init; } = [];
}

public record PermisoRolRecursoDto
{
    public int RecursoId { get; init; }
    public bool CanCreate { get; init; }
    public bool CanEdit { get; init; }
    public bool CanDelete { get; init; }
    public int Alcance { get; init; }
}
