namespace ERPNet.Application.Common.DTOs;

public class PermisoRolRecursoResponse
{
    public int RecursoId { get; set; }
    public string RecursoCodigo { get; set; } = null!;
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public int Alcance { get; set; }  // 0=Propio, 1=Seccion, 2=Global
}
