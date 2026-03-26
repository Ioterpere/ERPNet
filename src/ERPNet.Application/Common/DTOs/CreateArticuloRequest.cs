namespace ERPNet.Application.Common.DTOs;

public class CreateArticuloRequest
{
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? UnidadMedida { get; set; }
    public int? FamiliaArticuloId { get; set; }
    public bool EsInventariable { get; set; } = true;
}
