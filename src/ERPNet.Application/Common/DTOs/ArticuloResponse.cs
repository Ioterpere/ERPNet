namespace ERPNet.Application.Common.DTOs;

public record ArticuloResponse
{
    public int Id { get; init; }

    // Identificación
    public required string Codigo { get; init; }
    public string? CodigoBarras { get; init; }

    // Descripción
    public required string Descripcion { get; init; }
    public string? DescripcionVenta { get; init; }

    // Unidades
    public string? UnidadMedida { get; init; }
    public string? UnidadMedida2 { get; init; }

    // Precios
    public decimal PrecioCoste { get; init; }
    public decimal PrecioMedio { get; init; }
    public decimal PrecioVenta { get; init; }

    // Stock y logística
    public decimal StockMinimo { get; init; }
    public decimal NivelPedido { get; init; }
    public decimal NivelReposicion { get; init; }
    public decimal UnidadesCaja { get; init; }
    public int UnidadesPalet { get; init; }
    public int FilasPalet { get; init; }
    public decimal PesoGramo { get; init; }
    public int? LeadTime { get; init; }
    public int DiasVidaUtil { get; init; }
    public decimal Depreciacion { get; init; }

    // Flags
    public bool EsInventariable { get; init; }
    public bool EsPropio { get; init; }
    public bool EsNuevo { get; init; }
    public bool EsObsoleto { get; init; }

    // Texto libre
    public string? ProveedorPrincipal { get; init; }
    public string? Observaciones { get; init; }

    // Relaciones
    public int EmpresaId { get; init; }
    public int? FamiliaArticuloId { get; init; }
    public string? FamiliaArticuloNombre { get; init; }
    public int? TipoIvaId { get; init; }
    public string? TipoIvaNombre { get; init; }
    public int? FormatoArticuloId { get; init; }
    public string? FormatoArticuloNombre { get; init; }
    public int? ConfiguracionCaducidadId { get; init; }
    public string? ConfiguracionCaducidadNombre { get; init; }
}
