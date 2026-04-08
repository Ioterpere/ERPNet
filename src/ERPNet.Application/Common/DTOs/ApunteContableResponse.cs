namespace ERPNet.Application.Common.DTOs;

public record ApunteContableResponse
{
    public int Id { get; init; }
    public int CuentaId { get; init; }
    public string CodigoCuenta { get; init; } = string.Empty;
    public int? TipoDiarioId { get; init; }
    public string? CodigoTipoDiario { get; init; }
    public string? DescripcionTipoDiario { get; init; }
    public int? CentroCosteId { get; init; }
    public string? CodigoCentroCoste { get; init; }
    public string? DescripcionCentroCoste { get; init; }
    public int Asiento { get; init; }
    public int NumLinea { get; init; }
    public int NumDiario { get; init; }
    public DateOnly Fecha { get; init; }
    public string Concepto { get; init; } = string.Empty;
    public decimal Debe { get; init; }
    public decimal Haber { get; init; }
    public bool EsDefinitivo { get; init; }
    public int? IdPunteo { get; init; }
    public DateOnly? FechaPunteo { get; init; }
}
