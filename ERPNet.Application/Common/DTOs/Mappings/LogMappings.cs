using ERPNet.Domain.Entities;

namespace ERPNet.Application.Common.DTOs.Mappings;

public static class LogMappings
{
    public static LogResponse ToResponse(this Log log) => new()
    {
        Id = log.Id,
        UsuarioId = log.UsuarioId,
        Accion = log.Accion,
        Entidad = log.Entidad,
        EntidadId = log.EntidadId,
        Fecha = log.Fecha,
        Detalle = log.Detalle
    };
}
