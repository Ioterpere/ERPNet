namespace ERPNet.Contracts.DTOs;

public class CreateRolRequest
{
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
}
