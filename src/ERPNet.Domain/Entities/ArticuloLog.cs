namespace ERPNet.Domain.Entities;

public class ArticuloLog
{
    public long Id { get; set; }
    public int ArticuloId { get; set; }
    public Articulo Articulo { get; set; } = null!;

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public DateOnly Fecha { get; set; }
    public string Nota { get; set; } = null!;
    public decimal? StockAnterior { get; set; }
    public decimal? StockNuevo { get; set; }
    public DateTime CreatedAt { get; set; }
}
