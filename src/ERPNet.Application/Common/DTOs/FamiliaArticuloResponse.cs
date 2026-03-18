namespace ERPNet.Application.Common.DTOs;

public record FamiliaArticuloResponse
{
    public int Id { get; init; }
    public required string Nombre { get; init; }
    public string? Descripcion { get; init; }
    public int EmpresaId { get; init; }
    public int? FamiliaPadreId { get; init; }
    public string? FamiliaPadreNombre { get; init; }
}
