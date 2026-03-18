namespace ERPNet.Application.Common.DTOs;

public record ArticuloResponse
{
    public int Id { get; init; }
    public required string Codigo { get; init; }
    public required string Descripcion { get; init; }
    public string? UnidadMedida { get; init; }
    public decimal? PrecioCompra { get; init; }
    public decimal? PrecioVenta { get; init; }
    public bool Activo { get; init; }
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
