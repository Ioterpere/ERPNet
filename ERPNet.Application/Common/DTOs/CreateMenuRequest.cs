using ERPNet.Domain.Enums;

namespace ERPNet.Application.Common.DTOs;

public class CreateMenuRequest
{
    public string Nombre { get; set; } = null!;
    public string? Path { get; set; }
    public string? IconClass { get; set; }
    public string? CustomClass { get; set; }
    public int Orden { get; set; }
    public Plataforma Plataforma { get; set; }
    public int? MenuPadreId { get; set; }
    public int? RecursoId { get; set; }
}
