namespace ERPNet.Domain.Entities;

public class LogIntentoLogin
{
    public long Id { get; set; }
    public string NombreUsuario { get; set; } = null!; // Email intentado
    public string DireccionIp { get; set; } = null!;
    public DateTime FechaIntento { get; set; }
    public bool Exitoso { get; set; }

    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
}
