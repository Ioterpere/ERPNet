using ERPNet.Domain.Common.Values;
using ERPNet.Domain.Entities;

namespace ERPNet.Application.Common.DTOs.Mappings;

public static class EmpleadoMappings
{
    public static EmpleadoResponse ToResponse(this Empleado empleado) => new()
    {
        Id = empleado.Id,
        Nombre = empleado.Nombre,
        Apellidos = empleado.Apellidos,
        Dni = empleado.DNI.Value,
        Activo = empleado.Activo,
        SeccionId = empleado.SeccionId,
        SeccionNombre = empleado.Seccion?.Nombre ?? string.Empty,
        EncargadoId = empleado.EncargadoId,
        FotoId = empleado.FotoId
    };

    public static Empleado ToEntity(this CreateEmpleadoRequest request) => new()
    {
        Nombre = request.Nombre,
        Apellidos = request.Apellidos,
        DNI = Dni.From(request.Dni),
        Activo = true,
        SeccionId = request.SeccionId,
        EncargadoId = request.EncargadoId
    };

    public static void ApplyTo(this UpdateEmpleadoRequest request, Empleado empleado)
    {
        if (request.Nombre is not null)
            empleado.Nombre = request.Nombre;

        if (request.Apellidos is not null)
            empleado.Apellidos = request.Apellidos;

        if (request.Dni is not null)
            empleado.DNI = Dni.From(request.Dni);

        if (request.Activo.HasValue)
            empleado.Activo = request.Activo.Value;

        if (request.SeccionId.HasValue)
            empleado.SeccionId = request.SeccionId.Value;

        if (request.EncargadoId.HasValue)
            empleado.EncargadoId = request.EncargadoId.Value;
    }
}
