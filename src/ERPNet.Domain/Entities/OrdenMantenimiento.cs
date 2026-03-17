using ERPNet.Domain.Common;
using ERPNet.Domain.Enums;

namespace ERPNet.Domain.Entities;

public class OrdenMantenimiento : BaseEntity
{
    public int MaquinariaId { get; set; }
    public Maquinaria Maquinaria { get; set; } = null!;

    public int TipoMantenimientoId { get; set; }
    public TipoMantenimiento TipoMantenimiento { get; set; } = null!;

    public string Descripcion { get; set; } = null!;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaCierre { get; set; }
    public EstadoOrden Estado { get; set; }

    public int? AsignadoAId { get; set; }
    public Empleado? AsignadoA { get; set; }
}
