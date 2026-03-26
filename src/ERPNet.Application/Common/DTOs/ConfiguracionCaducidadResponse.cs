namespace ERPNet.Application.Common.DTOs;

public record ConfiguracionCaducidadResponse
{
    public int Id { get; init; }
    public required string Nombre { get; init; }
    public int DiasAviso { get; init; }
}
