namespace ERPNet.Application.Common.DTOs;

public record CreateRolRequest
{
    public required string Nombre { get; init; }
    public string? Descripcion { get; init; }
}
