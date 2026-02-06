using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class Empleado : BaseEntity
{
    public string Nombre { get; set; } = null!;
    public string Apellidos { get; set; } = null!;
    public string DNI { get; set; } = null!;
    public bool Activo { get; set; }

    public int SeccionId { get; set; }
    public Seccion Seccion { get; set; } = null!;

    public int? EncargadoId { get; set; }
    public Empleado? Encargado { get; set; }

    public Usuario? Usuario { get; set; }
    public ICollection<Empleado> Subordinados { get; set; } = [];
    public ICollection<AsignacionTurno> AsignacionesTurno { get; set; } = [];
    public ICollection<Vacacion> Vacaciones { get; set; } = [];
    public ICollection<Marcaje> Marcajes { get; set; } = [];
}
