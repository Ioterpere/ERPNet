namespace ERPNet.Application.Common.DTOs;

public record CambiarContrasenaRequest
{
    public required string ContrasenaActual { get; init; }
    public required string NuevaContrasena { get; init; }
    public required string ConfirmarContrasena { get; init; }
}
