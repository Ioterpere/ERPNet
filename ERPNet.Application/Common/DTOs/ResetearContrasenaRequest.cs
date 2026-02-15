namespace ERPNet.Application.Common.DTOs;

public class ResetearContrasenaRequest
{
    public string NuevaContrasena { get; set; } = null!;
    public string ConfirmarContrasena { get; set; } = null!;
}
