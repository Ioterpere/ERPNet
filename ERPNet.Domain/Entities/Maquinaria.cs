using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class Maquinaria : BaseEntity
{
    public string Nombre { get; set; } = null!;
    public string Codigo { get; set; } = null!;
    public string? Ubicacion { get; set; }
    public bool Activa { get; set; }

    public int? SeccionId { get; set; }
    public Seccion? Seccion { get; set; }

    public ICollection<OrdenMantenimiento> OrdenesMantenimiento { get; set; } = [];
}
