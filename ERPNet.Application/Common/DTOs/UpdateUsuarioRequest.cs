namespace ERPNet.Application.Common.DTOs;

public class UpdateUsuarioRequest
{
    public string? Email { get; set; }
    public int? EmpleadoId { get; set; }
    public bool? Activo { get; set; }
    public DateTime? CaducidadContrasena { get; set; }
}
