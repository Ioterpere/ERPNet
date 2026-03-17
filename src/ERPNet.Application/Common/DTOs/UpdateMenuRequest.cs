namespace ERPNet.Application.Common.DTOs;

public record UpdateMenuRequest
{
    public required string Nombre { get; init; }
    public string? Path { get; init; }
    public string? IconClass { get; init; }
    public string? CustomClass { get; init; }
    public int Orden { get; init; }
}
