namespace ERPNet.Contracts.DTOs;

public class RolResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
}
