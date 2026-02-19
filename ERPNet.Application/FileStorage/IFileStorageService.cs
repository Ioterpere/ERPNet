using ERPNet.Contracts;
using ERPNet.Contracts.FileStorage;

namespace ERPNet.Application.FileStorage;

public interface IFileStorageService
{
    Task<Result<ArchivoResponse>> SubirAsync(Stream contenido, string nombreArchivo, string contentType, CancellationToken ct = default);
    Task<Result<ArchivoDescarga>> DescargarAsync(Guid archivoId, CancellationToken ct = default);
    Task<Result<ArchivoDescarga>> DescargarThumbnailAsync(Guid archivoId, CancellationToken ct = default);
    Task<Result<ArchivoResponse>> ObtenerMetadataAsync(Guid archivoId, CancellationToken ct = default);
    Task<Result> EliminarAsync(Guid archivoId, CancellationToken ct = default);
}
