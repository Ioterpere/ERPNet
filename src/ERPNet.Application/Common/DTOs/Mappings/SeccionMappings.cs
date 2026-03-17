using ERPNet.Domain.Entities;

namespace ERPNet.Application.Common.DTOs.Mappings;

public static class SeccionMappings
{
    public static SeccionResponse ToResponse(this Seccion seccion) => new()
    {
        Id = seccion.Id,
        Nombre = seccion.Nombre
    };
}
