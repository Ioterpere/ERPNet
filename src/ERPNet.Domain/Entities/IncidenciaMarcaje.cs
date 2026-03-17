using ERPNet.Domain.Common;
using ERPNet.Domain.Enums;

namespace ERPNet.Domain.Entities;

public class IncidenciaMarcaje : BaseEntity
{
    public int MarcajeId { get; set; }
    public Marcaje Marcaje { get; set; } = null!;

    public int MinutosRetraso { get; set; }
    public EstadoIncidencia Estado { get; set; }
    public string? Observaciones { get; set; }

    public int? ValidadaPorId { get; set; }
    public Empleado? ValidadaPor { get; set; }
}
