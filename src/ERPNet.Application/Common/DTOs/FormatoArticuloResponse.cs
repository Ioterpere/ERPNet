namespace ERPNet.Application.Common.DTOs;

public record FormatoArticuloResponse
{
    public int Id { get; init; }
    public required string Nombre { get; init; }
}
