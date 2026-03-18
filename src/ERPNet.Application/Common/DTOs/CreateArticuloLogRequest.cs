namespace ERPNet.Application.Common.DTOs;

public class CreateArticuloLogRequest
{
    public DateOnly Fecha { get; set; }
    public string Nota { get; set; } = string.Empty;
    public decimal? StockAnterior { get; set; }
    public decimal? StockNuevo { get; set; }
}
