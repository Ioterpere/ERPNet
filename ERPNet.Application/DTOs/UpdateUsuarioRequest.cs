namespace ERPNet.Application.DTOs;

public class UpdateUsuarioRequest
{
    public string? Email { get; set; }
    public int? EmpleadoId { get; set; }
    public bool? Activo { get; set; }
}
