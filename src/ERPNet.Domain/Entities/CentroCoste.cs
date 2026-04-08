using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class CentroCoste : BaseEntity, IPerteneceEmpresa
{
    public string Codigo { get; set; } = null!;      // max 4, e.g. "ADM"
    public string Descripcion { get; set; } = null!; // max 50

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public ICollection<ApunteContable> Apuntes { get; set; } = [];
}
