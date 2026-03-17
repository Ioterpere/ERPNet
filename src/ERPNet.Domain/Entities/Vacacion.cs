using ERPNet.Domain.Common;
using ERPNet.Domain.Enums;

namespace ERPNet.Domain.Entities;

public class Vacacion : BaseEntity
{
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public EstadoVacaciones Estado { get; set; }

    public int? AprobadoPorId { get; set; }
    public Empleado? AprobadoPor { get; set; }

    public string? Observaciones { get; set; }
}
