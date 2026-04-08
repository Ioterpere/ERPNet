namespace ERPNet.Application.Common.DTOs;

public record TipoDiarioResponse
{
    public int Id { get; init; }
    public required string Codigo { get; init; }
    public required string Descripcion { get; init; }
    public bool EsNoOficial { get; init; }
}
