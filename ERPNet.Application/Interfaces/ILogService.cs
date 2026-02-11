namespace ERPNet.Application.Interfaces;

public interface ILogService
{
    Task EventAsync(string accion, string detalle, int? usuarioId = null, CancellationToken ct = default);
    Task ErrorAsync(Exception ex, string? codigoError = null, CancellationToken ct = default);
    Task WarningAsync(string detalle, CancellationToken ct = default);
    void Entidad(string accion, string entidad, string? entidadId, int? usuarioId, string? detalle);
}
