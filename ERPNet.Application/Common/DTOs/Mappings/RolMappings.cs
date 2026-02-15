using ERPNet.Domain.Entities;

namespace ERPNet.Application.Common.DTOs.Mappings;

public static class RolMappings
{
    public static RolResponse ToResponse(this Rol rol) => new()
    {
        Id = rol.Id,
        Nombre = rol.Nombre,
        Descripcion = rol.Descripcion
    };

    public static Rol ToEntity(this CreateRolRequest request) => new()
    {
        Nombre = request.Nombre,
        Descripcion = request.Descripcion
    };

    public static void ApplyTo(this UpdateRolRequest request, Rol rol)
    {
        if (request.Nombre is not null)
            rol.Nombre = request.Nombre;

        if (request.Descripcion is not null)
            rol.Descripcion = request.Descripcion;
    }
}
