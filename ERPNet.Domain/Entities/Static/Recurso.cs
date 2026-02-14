using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class Recurso : StaticEntity
{
    public ICollection<PermisoRolRecurso> PermisosRolRecurso { get; set; } = [];
}
