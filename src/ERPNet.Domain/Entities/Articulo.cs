using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class Articulo : BaseEntity, IPerteneceEmpresa
{
    public string Codigo { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public string? UnidadMedida { get; set; }
    public decimal? PrecioCompra { get; set; }
    public decimal? PrecioVenta { get; set; }
    public bool Activo { get; set; }

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public int? FamiliaArticuloId { get; set; }
    public FamiliaArticulo? FamiliaArticulo { get; set; }

    public int? TipoIvaId { get; set; }
    public TipoIva? TipoIva { get; set; }

    public int? FormatoArticuloId { get; set; }
    public FormatoArticulo? FormatoArticulo { get; set; }

    public int? ConfiguracionCaducidadId { get; set; }
    public ConfiguracionCaducidad? ConfiguracionCaducidad { get; set; }

    public ICollection<ArticuloLog> Logs { get; set; } = [];
}
