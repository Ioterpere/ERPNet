namespace ERPNet.Application.Common.DTOs;

public class UpdateMaquinariaRequest
{
    public string? Nombre { get; set; }
    public string? Codigo { get; set; }
    public string? Ubicacion { get; set; }
    public bool? Activa { get; set; }
    public int? SeccionId { get; set; }
}
