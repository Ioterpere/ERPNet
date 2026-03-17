using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class TipoMantenimiento : StaticEntity
{
    public ICollection<OrdenMantenimiento> OrdenesMantenimiento { get; set; } = [];
}
