using ERPNet.Application;
using ERPNet.Application.Enums;
using ERPNet.Application.Reports;
using ERPNet.Application.Reports.DTOs;
using ERPNet.Application.Reports.Interfaces;
using ERPNet.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace ERPNet.Infrastructure.Reports;

public class ReporteEmpleadoService(ERPNetDbContext context) : IReporteEmpleadoService
{
    public async Task<Result<ReporteArchivo>> GenerarAsync(
        EmpleadoReporteFilter filter, CancellationToken ct = default)
    {
        var query = context.Empleados
            .Include(e => e.Seccion)
            .AsNoTracking();

        if (filter.SeccionId.HasValue)
            query = query.Where(e => e.SeccionId == filter.SeccionId.Value);

        if (filter.Activo.HasValue)
            query = query.Where(e => e.Activo == filter.Activo.Value);

        var empleados = await query
            .OrderBy(e => e.Apellidos).ThenBy(e => e.Nombre)
            .ToListAsync(ct);

        if (empleados.Count == 0)
            return Result<ReporteArchivo>.Failure("No se encontraron empleados con los filtros indicados.", ErrorType.NotFound);

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        return filter.Formato switch
        {
            FormatoReporte.Pdf => Result<ReporteArchivo>.Success(
                new ReporteArchivo(
                    new EmpleadoReportePdf(empleados).GeneratePdf(),
                    "application/pdf",
                    $"empleados_{timestamp}.pdf")),

            FormatoReporte.Excel => Result<ReporteArchivo>.Success(
                new ReporteArchivo(
                    EmpleadoReporteExcel.Generar(empleados),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"empleados_{timestamp}.xlsx")),

            _ => Result<ReporteArchivo>.Failure("Formato no soportado.", ErrorType.Validation)
        };
    }
}
