using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class Usuario : BaseEntity
{
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime? UltimoAcceso { get; set; }
    public bool Activo { get; set; }

    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public ICollection<RolUsuario> RolesUsuarios { get; set; } = [];
    public ICollection<Log> Logs { get; set; } = [];
}
