namespace ERPNet.Application.Common.DTOs;

public record CreateUsuarioRequest
{
    public required string Email { get; init; }
    public int EmpleadoId { get; init; }
}
