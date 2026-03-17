namespace ERPNet.Application.Ai.DTOs;

public record ChatResponse
{
    public required string Texto { get; init; }
    public AccionUi? Accion { get; init; }
}
