using ERPNet.Domain.Common;
using ERPNet.Domain.Enums;

namespace ERPNet.Domain.Entities;

public class Empleado : BaseEntity, IHasArchivos<CampoArchivoEmpleado>
{
    public string Nombre { get; set; } = null!;
    public string Apellidos { get; set; } = null!;
    public string DNI { get; set; } = null!;
    public bool Activo { get; set; }

    public int SeccionId { get; set; }
    public Seccion Seccion { get; set; } = null!;

    public int? EncargadoId { get; set; }
    public Empleado? Encargado { get; set; }

    public Guid? FotoId { get; set; }
    public Archivo? Foto { get; set; }

    public Usuario? Usuario { get; set; }
    public ICollection<Empleado> Subordinados { get; set; } = [];
    public ICollection<AsignacionTurno> AsignacionesTurno { get; set; } = [];
    public ICollection<Vacacion> Vacaciones { get; set; } = [];
    public ICollection<Marcaje> Marcajes { get; set; } = [];

    public Guid? GetArchivoId(CampoArchivoEmpleado campo) => campo switch
    {
        CampoArchivoEmpleado.Foto => FotoId,
        _ => null
    };

    public void SetArchivoId(CampoArchivoEmpleado campo, Guid? id)
    {
        switch (campo)
        {
            case CampoArchivoEmpleado.Foto: FotoId = id; break;
        }
    }
}
