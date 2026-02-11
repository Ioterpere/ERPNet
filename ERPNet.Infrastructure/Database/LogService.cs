using ERPNet.Application.Interfaces;
using ERPNet.Domain.Entities;
using ERPNet.Infrastructure.Database.Context;

namespace ERPNet.Infrastructure.Database;

public class LogService(ERPNetDbContext context) : ILogService
{
    public async Task ErrorAsync(Exception ex, string? codigoError = null, CancellationToken ct = default)
    {
        context.Logs.Add(new Log
        {
            Accion = "Excepcion",
            Entidad = ex.GetType().Name,
            Fecha = DateTime.UtcNow,
            Detalle = ex.Message,
            CodigoError = codigoError
        });

        await context.SaveChangesAsync(ct);
    }

    public async Task WarningAsync(string detalle, CancellationToken ct = default)
    {
        context.Logs.Add(new Log
        {
            Accion = "Warning",
            Fecha = DateTime.UtcNow,
            Detalle = detalle
        });

        await context.SaveChangesAsync(ct);
    }

    public void Entidad(string accion, string entidad, string? entidadId, int? usuarioId, string? detalle)
    {
        context.Logs.Add(new Log
        {
            UsuarioId = usuarioId,
            Accion = accion,
            Entidad = entidad,
            EntidadId = entidadId,
            Fecha = DateTime.UtcNow,
            Detalle = detalle
        });
    }
}
