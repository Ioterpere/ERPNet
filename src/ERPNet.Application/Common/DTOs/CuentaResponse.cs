namespace ERPNet.Application.Common.DTOs;

public record CuentaResponse
{
    public int Id { get; init; }
    public required string Codigo { get; init; }
    public required string Descripcion { get; init; }
    public string? DescripcionSII { get; init; }
    public string? Nif { get; init; }
    public bool EsNoOficial { get; init; }
    public bool EsObsoleta { get; init; }
    public int EmpresaId { get; init; }
    public int? CuentaPadreId { get; init; }
    public string? CuentaPadreCodigo { get; init; }
    public string? CuentaPadreDescripcion { get; init; }
    public string? CuentaPadreDescripcionSII { get; init; }

    // Parámetros asociados
    public int? CuentaAmortizacionId { get; init; }
    public string? CuentaAmortizacionCodigo { get; init; }
    public string? CuentaAmortizacionDescripcion { get; init; }

    public int? CuentaPagoDelegadoId { get; init; }
    public string? CuentaPagoDelegadoCodigo { get; init; }
    public string? CuentaPagoDelegadoDescripcion { get; init; }

    public int? EmpresaVinculadaId { get; init; }
    public string? EmpresaVinculadaNombre { get; init; }

    public int? ConceptoAnaliticaId { get; init; }
    public int? ClienteAsociadoId { get; init; }
}
