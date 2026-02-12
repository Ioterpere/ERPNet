namespace ERPNet.Domain.Entities;

/// <summary>
/// No hereda de BaseEntity porque usa Guid como PK y gestiona
/// su auditoría manualmente desde MinioFileStorageService
/// (UnitOfWork solo audita entidades BaseEntity con int PK).
/// </summary>
public class Archivo
{
    public Guid Id { get; set; }
    public string NombreOriginal { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Tamaño { get; set; }

    // Thumbnails
    public bool EsThumbnail { get; set; }
    public Guid? ArchivoOriginalId { get; set; }
    public Archivo? ArchivoOriginal { get; set; }
    public ICollection<Archivo> Thumbnails { get; set; } = [];

    // Auditoría
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public int? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
}
