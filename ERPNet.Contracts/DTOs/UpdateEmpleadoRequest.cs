namespace ERPNet.Contracts.DTOs;

public class UpdateEmpleadoRequest
{
    public string? Nombre { get; set; }
    public string? Apellidos { get; set; }
    public string? Dni { get; set; }
    public bool? Activo { get; set; }
    public int? SeccionId { get; set; }
    public int? EncargadoId { get; set; }
}
