namespace ERPNet.Contracts.DTOs;

public class MaquinariaResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Codigo { get; set; } = null!;
    public string? Ubicacion { get; set; }
    public bool Activa { get; set; }
    public int? SeccionId { get; set; }
    public Guid? FichaTecnicaId { get; set; }
    public Guid? ManualId { get; set; }
    public Guid? CertificadoCeId { get; set; }
    public Guid? FotoId { get; set; }
}
