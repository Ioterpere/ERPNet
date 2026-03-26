namespace ERPNet.Application.Common.DTOs;

public class UpdateArticuloRequest
{
    // Identificación
    public string? Codigo { get; set; }
    public string? CodigoBarras { get; set; }

    // Descripción
    public string? Descripcion { get; set; }
    public string? DescripcionVenta { get; set; }

    // Unidades
    public string? UnidadMedida { get; set; }
    public string? UnidadMedida2 { get; set; }

    // Precios
    public decimal? PrecioCoste { get; set; }
    public decimal? PrecioMedio { get; set; }
    public decimal? PrecioVenta { get; set; }

    // Stock y logística
    public decimal? StockMinimo { get; set; }
    public decimal? NivelPedido { get; set; }
    public decimal? NivelReposicion { get; set; }
    public decimal? UnidadesCaja { get; set; }
    public int? UnidadesPalet { get; set; }
    public int? FilasPalet { get; set; }
    public decimal? PesoGramo { get; set; }
    public int? LeadTime { get; set; }
    public int? DiasVidaUtil { get; set; }
    public decimal? Depreciacion { get; set; }

    // Flags
    public bool? EsInventariable { get; set; }
    public bool? EsPropio { get; set; }
    public bool? EsNuevo { get; set; }
    public bool? EsObsoleto { get; set; }

    // Texto libre
    public string? ProveedorPrincipal { get; set; }
    public string? Observaciones { get; set; }

    // Relaciones
    public int? FamiliaArticuloId { get; set; }
    public int? TipoIvaId { get; set; }
    public int? FormatoArticuloId { get; set; }
    public int? ConfiguracionCaducidadId { get; set; }
}
