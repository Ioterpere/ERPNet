using ERPNet.Domain.Common;
using ERPNet.Domain.Enums;

namespace ERPNet.Domain.Entities;

public class Maquinaria : BaseEntity, IHasArchivos<CampoArchivoMaquinaria>
{
    public string Nombre { get; set; } = null!;
    public string Codigo { get; set; } = null!;
    public string? Ubicacion { get; set; }
    public bool Activa { get; set; }

    public int? SeccionId { get; set; }
    public Seccion? Seccion { get; set; }

    public Guid? FichaTecnicaId { get; set; }
    public Archivo? FichaTecnica { get; set; }
    public Guid? ManualId { get; set; }
    public Archivo? Manual { get; set; }
    public Guid? CertificadoCeId { get; set; }
    public Archivo? CertificadoCe { get; set; }
    public Guid? FotoId { get; set; }
    public Archivo? Foto { get; set; }

    public ICollection<OrdenMantenimiento> OrdenesMantenimiento { get; set; } = [];

    public Guid? GetArchivoId(CampoArchivoMaquinaria campo) => campo switch
    {
        CampoArchivoMaquinaria.FichaTecnica => FichaTecnicaId,
        CampoArchivoMaquinaria.Manual => ManualId,
        CampoArchivoMaquinaria.CertificadoCe => CertificadoCeId,
        CampoArchivoMaquinaria.Foto => FotoId,
        _ => null
    };

    public void SetArchivoId(CampoArchivoMaquinaria campo, Guid? id)
    {
        switch (campo)
        {
            case CampoArchivoMaquinaria.FichaTecnica: FichaTecnicaId = id; break;
            case CampoArchivoMaquinaria.Manual: ManualId = id; break;
            case CampoArchivoMaquinaria.CertificadoCe: CertificadoCeId = id; break;
            case CampoArchivoMaquinaria.Foto: FotoId = id; break;
        }
    }
}
