namespace ERPNet.Domain.Entities;

public class TipoIva
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public decimal Porcentaje { get; set; }
}
