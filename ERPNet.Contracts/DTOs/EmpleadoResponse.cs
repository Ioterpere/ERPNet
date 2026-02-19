namespace ERPNet.Contracts.DTOs;

public class EmpleadoResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Apellidos { get; set; } = null!;
    public string Dni { get; set; } = null!;
    public bool Activo { get; set; }
    public int SeccionId { get; set; }
    public int? EncargadoId { get; set; }
    public Guid? FotoId { get; set; }
}
