using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class FamiliaArticulo : BaseEntity, IPerteneceEmpresa
{
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public int? FamiliaPadreId { get; set; }
    public FamiliaArticulo? FamiliaPadre { get; set; }

    public ICollection<FamiliaArticulo> SubFamilias { get; set; } = [];
    public ICollection<Articulo> Articulos { get; set; } = [];
}
