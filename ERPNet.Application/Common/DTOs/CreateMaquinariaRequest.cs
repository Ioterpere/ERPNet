namespace ERPNet.Application.Common.DTOs;

public class CreateMaquinariaRequest
{
    public string Nombre { get; set; } = null!;
    public string Codigo { get; set; } = null!;
    public string? Ubicacion { get; set; }
    public int? SeccionId { get; set; }
}
