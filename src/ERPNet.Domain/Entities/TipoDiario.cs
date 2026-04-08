using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class TipoDiario : BaseEntity, IPerteneceEmpresa
{
    public string Codigo { get; set; } = null!;      // max 2, e.g. "AP", "VT"
    public string Descripcion { get; set; } = null!; // max 25
    public bool EsNoOficial { get; set; }

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public ICollection<ApunteContable> Apuntes { get; set; } = [];
}
