namespace ERPNet.Contracts.DTOs;

public class UsuarioResponse
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public int EmpleadoId { get; set; }
    public bool Activo { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    public DateTime? CaducidadContrasena { get; set; }
    public DateTime UltimoCambioContrasena { get; set; }
}
