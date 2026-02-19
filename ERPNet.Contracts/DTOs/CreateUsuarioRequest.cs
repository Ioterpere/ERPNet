namespace ERPNet.Contracts.DTOs;

public class CreateUsuarioRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public int EmpleadoId { get; set; }
}
