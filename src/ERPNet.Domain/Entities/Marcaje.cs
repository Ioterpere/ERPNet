using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class Marcaje : BaseEntity
{
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public DateOnly Fecha { get; set; }
    public DateTime? HoraEntrada { get; set; }
    public DateTime? HoraSalida { get; set; }

    public IncidenciaMarcaje? Incidencia { get; set; }
}
