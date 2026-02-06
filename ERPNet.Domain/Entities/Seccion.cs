using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class Seccion : BaseEntity
{
    public string Nombre { get; set; } = null!;
    public int? ResponsableId { get; set; }

    public Empleado? Responsable { get; set; }
    public ICollection<Empleado> Empleados { get; set; } = [];
    public ICollection<Maquinaria> Maquinarias { get; set; } = [];
}
