using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class TipoMantenimiento : BaseEntity
{
    public string Nombre { get; set; } = null!;

    public ICollection<OrdenMantenimiento> OrdenesMantenimiento { get; set; } = [];
}
