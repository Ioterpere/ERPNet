namespace ERPNet.Contracts.FileStorage;

public class ArchivoResponse
{
    public Guid Id { get; set; }
    public string NombreOriginal { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Tamaño { get; set; }
    public bool EsThumbnail { get; set; }
    public Guid? ThumbnailId { get; set; }
    public DateTime CreatedAt { get; set; }
}
