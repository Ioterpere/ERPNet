using ERPNet.Domain.Enums;

namespace ERPNet.Application.Common.DTOs;

public record CreateMenuRequest
{
    public required string Nombre { get; init; }
    public string? Path { get; init; }
    public string? IconClass { get; init; }
    public string? CustomClass { get; init; }
    public int Orden { get; init; }
    public Plataforma Plataforma { get; init; }
    public int? MenuPadreId { get; init; }
    public List<int> RolIds { get; init; } = [];
}
