namespace ERPNet.Contracts.DTOs;

public class CambiarContrasenaRequest
{
    public string ContrasenaActual { get; set; } = null!;
    public string NuevaContrasena { get; set; } = null!;
    public string ConfirmarContrasena { get; set; } = null!;
}
