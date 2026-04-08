namespace ERPNet.Application.Common.DTOs;

public record SaldoMensualResponse
{
    public int Mes { get; init; }
    public decimal Debe { get; init; }
    public decimal Haber { get; init; }
    public decimal SaldoMes { get; init; }
    public decimal SaldoAcumulado { get; init; }
    public int NumApuntes { get; init; }
}
