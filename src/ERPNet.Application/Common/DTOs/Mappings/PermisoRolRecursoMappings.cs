using ERPNet.Domain.Entities;

namespace ERPNet.Application.Common.DTOs.Mappings;

public static class PermisoRolRecursoMappings
{
    public static PermisoRolRecursoResponse ToResponse(this PermisoRolRecurso permiso) => new()
    {
        RecursoId = permiso.RecursoId,
        RecursoCodigo = permiso.Recurso.Codigo,
        CanCreate = permiso.CanCreate,
        CanEdit = permiso.CanEdit,
        CanDelete = permiso.CanDelete,
        Alcance = (int)permiso.Alcance
    };
}
