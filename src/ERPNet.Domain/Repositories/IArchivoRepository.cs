using ERPNet.Domain.Entities;

namespace ERPNet.Domain.Repositories;

public interface IArchivoRepository
{
    Task<Archivo?> GetByIdAsync(Guid id);
    Task<Archivo?> GetByIdConThumbnailsAsync(Guid id);
    Task<Archivo> CreateAsync(Archivo archivo);
}
