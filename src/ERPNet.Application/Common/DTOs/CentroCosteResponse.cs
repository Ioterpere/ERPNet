namespace ERPNet.Application.Common.DTOs;

public record CentroCosteResponse
{
    public int Id { get; init; }
    public required string Codigo { get; init; }
    public required string Descripcion { get; init; }
}
