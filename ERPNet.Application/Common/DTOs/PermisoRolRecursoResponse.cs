namespace ERPNet.Application.Common.DTOs;

public record PermisoRolRecursoResponse
{
    public int RecursoId { get; init; }
    public required string RecursoCodigo { get; init; }
    public bool CanCreate { get; init; }
    public bool CanEdit { get; init; }
    public bool CanDelete { get; init; }
    public int Alcance { get; init; }  // 0=Propio, 1=Seccion, 2=Global
}
