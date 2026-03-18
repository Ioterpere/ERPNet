namespace ERPNet.Application.Common.DTOs;

public class CreateArticuloRequest
{
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? UnidadMedida { get; set; }
    public decimal? PrecioCompra { get; set; }
    public decimal? PrecioVenta { get; set; }
    public int? FamiliaArticuloId { get; set; }
    public int? TipoIvaId { get; set; }
    public int? FormatoArticuloId { get; set; }
    public int? ConfiguracionCaducidadId { get; set; }
}
