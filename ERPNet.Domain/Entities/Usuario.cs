using ERPNet.Domain.Common;
using ERPNet.Domain.Common.Values;

namespace ERPNet.Domain.Entities;

public class Usuario : BaseEntity
{
    public Email Email { get; set; }
    public string PasswordHash { get; set; } = null!;
    public DateTime? UltimoAcceso { get; set; }
    public bool Activo { get; set; }

    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public ICollection<RolUsuario> RolesUsuarios { get; set; } = [];
    public ICollection<Log> Logs { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<LogIntentoLogin> IntentosLogin { get; set; } = [];
}
