using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class AsignacionTurno : BaseEntity
{
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public int TurnoId { get; set; }
    public Turno Turno { get; set; } = null!;

    public DateOnly FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }
}
