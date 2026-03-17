namespace ERPNet.Application.Common.DTOs;

public record RolResponse
{
    public int Id { get; init; }
    public required string Nombre { get; init; }
    public string? Descripcion { get; init; }
}
