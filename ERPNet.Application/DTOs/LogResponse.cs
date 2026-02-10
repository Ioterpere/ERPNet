namespace ERPNet.Application.DTOs;

public class LogResponse
{
    public long Id { get; set; }
    public int? UsuarioId { get; set; }
    public string Accion { get; set; } = null!;
    public string Entidad { get; set; } = null!;
    public string? EntidadId { get; set; }
    public DateTime Fecha { get; set; }
    public string? Detalle { get; set; }
}
