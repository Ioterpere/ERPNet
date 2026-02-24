namespace ERPNet.Application.Common.DTOs;

public record MaquinariaResponse
{
    public int Id { get; init; }
    public required string Nombre { get; init; }
    public required string Codigo { get; init; }
    public string? Ubicacion { get; init; }
    public bool Activa { get; init; }
    public int? SeccionId { get; init; }
    public Guid? FichaTecnicaId { get; init; }
    public Guid? ManualId { get; init; }
    public Guid? CertificadoCeId { get; init; }
    public Guid? FotoId { get; init; }
}
