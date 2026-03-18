namespace ERPNet.Application.Common.DTOs;

public class UpdateArticuloRequest
{
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public string? UnidadMedida { get; set; }
    public decimal? PrecioCompra { get; set; }
    public decimal? PrecioVenta { get; set; }
    public bool? Activo { get; set; }
    public int? FamiliaArticuloId { get; set; }
    public int? TipoIvaId { get; set; }
    public int? FormatoArticuloId { get; set; }
    public int? ConfiguracionCaducidadId { get; set; }
}
