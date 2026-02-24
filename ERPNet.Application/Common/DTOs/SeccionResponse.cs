namespace ERPNet.Application.Common.DTOs;

public record SeccionResponse
{
    public int Id { get; init; }
    public required string Nombre { get; init; }
}
