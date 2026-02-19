namespace ERPNet.Application.Common.DTOs;

public class CreateUsuarioRequest
{
    public string Email { get; set; } = null!;
    public int EmpleadoId { get; set; }
}
