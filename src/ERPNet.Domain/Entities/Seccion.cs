using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class Seccion : BaseEntity, IPerteneceEmpresa
{
    public string Nombre { get; set; } = null!;
    public int? ResponsableId { get; set; }

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Empleado? Responsable { get; set; }
    public ICollection<Empleado> Empleados { get; set; } = [];
    public ICollection<Maquinaria> Maquinarias { get; set; } = [];
}
