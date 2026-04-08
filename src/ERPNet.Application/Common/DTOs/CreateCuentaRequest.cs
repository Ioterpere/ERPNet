namespace ERPNet.Application.Common.DTOs;

public class CreateCuentaRequest
{
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? DescripcionSII { get; set; }
    public string? Nif { get; set; }
    public bool EsNoOficial { get; set; }
    public bool EsObsoleta { get; set; }
    public int? CuentaPadreId { get; set; }
}
