namespace ERPNet.Application.Common.DTOs;

public class CreateEmpleadoRequest
{
    public string Nombre { get; set; } = null!;
    public string Apellidos { get; set; } = null!;
    public string Dni { get; set; } = null!;
    public int SeccionId { get; set; }
    public int? EncargadoId { get; set; }
}
