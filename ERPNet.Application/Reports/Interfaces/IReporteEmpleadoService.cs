using ERPNet.Application.Reports.DTOs;

namespace ERPNet.Application.Reports.Interfaces;

public interface IReporteEmpleadoService
{
    Task<Result<ReporteArchivo>> GenerarAsync(EmpleadoReporteFilter filter, CancellationToken ct = default);
}
