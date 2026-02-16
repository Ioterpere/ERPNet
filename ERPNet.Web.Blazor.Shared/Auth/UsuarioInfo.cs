namespace ERPNet.Web.Blazor.Shared.Auth;

public class UsuarioInfo
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public int EmpleadoId { get; set; }
    public int SeccionId { get; set; }
    public List<int> Roles { get; set; } = [];
    public bool RequiereCambioContrasena { get; set; }
}
