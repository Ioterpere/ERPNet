namespace ERPNet.Application.Common.DTOs;

public record UpdateRolRequest
{
    public string? Nombre { get; init; }
    public string? Descripcion { get; init; }
}
