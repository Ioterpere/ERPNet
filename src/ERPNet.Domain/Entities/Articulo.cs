using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class Articulo : BaseEntity, IPerteneceEmpresa
{
    // ── Identificación ─────────────────────────────────────────────
    public string Codigo { get; set; } = null!;
    public string? CodigoBarras { get; set; }

    // ── Descripción ────────────────────────────────────────────────
    public string Descripcion { get; set; } = null!;
    public string? DescripcionVenta { get; set; }

    // ── Unidades ───────────────────────────────────────────────────
    public string? UnidadMedida { get; set; }
    public string? UnidadMedida2 { get; set; }

    // ── Precios ────────────────────────────────────────────────────
    public decimal PrecioCoste { get; set; }
    public decimal PrecioMedio { get; set; }
    public decimal PrecioVenta { get; set; }

    // ── Stock y logística ──────────────────────────────────────────
    public decimal StockMinimo { get; set; }
    public decimal NivelPedido { get; set; }
    public decimal NivelReposicion { get; set; }
    public decimal UnidadesCaja { get; set; }
    public int UnidadesPalet { get; set; }
    public int FilasPalet { get; set; }
    public decimal PesoGramo { get; set; }
    public int? LeadTime { get; set; }
    public int DiasVidaUtil { get; set; }
    public decimal Depreciacion { get; set; }

    // ── Flags ──────────────────────────────────────────────────────
    public bool EsInventariable { get; set; } = true;
    public bool EsPropio { get; set; } = true;
    public bool EsNuevo { get; set; }
    public bool EsObsoleto { get; set; }

    // ── Texto libre ────────────────────────────────────────────────
    public string? ProveedorPrincipal { get; set; }
    public string? Observaciones { get; set; }

    // ── Relaciones empresa ─────────────────────────────────────────
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    // ── Relaciones catálogo ────────────────────────────────────────
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
