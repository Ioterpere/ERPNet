namespace ERPNet.Application.Common.DTOs;

public record MoverMenuRequest
{
    public int? MenuPadreId { get; init; }
    public int Orden { get; init; }
}
