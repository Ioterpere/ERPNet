namespace ERPNet.Domain.Entities;

public class Log
{
    public long Id { get; set; }
    public int? UsuarioId { get; set; }
    public string Accion { get; set; } = null!;
    public string Entidad { get; set; } = null!;
    public string? EntidadId { get; set; }
    public DateTime Fecha { get; set; }
    public string? Detalle { get; set; }
    public string? CodigoError { get; set; }

    public Usuario? Usuario { get; set; }
}
