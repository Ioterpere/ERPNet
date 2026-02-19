namespace ERPNet.Contracts.Reports;

public class EmpleadoReporteFilter
{
    public FormatoReporte Formato { get; init; } = FormatoReporte.Pdf;
    public int? SeccionId { get; init; }
    public bool? Activo { get; init; }
}
