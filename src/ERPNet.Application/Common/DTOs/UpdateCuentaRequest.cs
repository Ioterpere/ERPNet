namespace ERPNet.Application.Common.DTOs;

public class UpdateCuentaRequest
{
    public string? Descripcion { get; set; }
    public string? DescripcionSII { get; set; }
    public string? Nif { get; set; }
    public bool? EsNoOficial { get; set; }
    public bool? EsObsoleta { get; set; }
    public int? CuentaPadreId { get; set; }
    public int? CuentaAmortizacionId { get; set; }
    public int? CuentaPagoDelegadoId { get; set; }
    public int? EmpresaVinculadaId { get; set; }
    public int? ConceptoAnaliticaId { get; set; }
    public int? ClienteAsociadoId { get; set; }
}
