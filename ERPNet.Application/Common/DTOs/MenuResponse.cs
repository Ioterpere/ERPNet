namespace ERPNet.Application.Common.DTOs;

public record MenuResponse
{
    public int Id { get; init; }
    public required string Nombre { get; init; }
    public string? Path { get; init; }
    public string? IconClass { get; init; }
    public string? CustomClass { get; init; }
    public int Orden { get; init; }
    public List<MenuResponse> SubMenus { get; init; } = [];
}
