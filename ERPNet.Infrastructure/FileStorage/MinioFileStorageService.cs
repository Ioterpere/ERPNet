using ERPNet.Application.Auth.Interfaces;
using ERPNet.Application.FileStorage;
using ERPNet.Application.FileStorage.DTOs.Mappings;
using ERPNet.Contracts;
using ERPNet.Contracts.FileStorage;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Repositories;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ERPNet.Infrastructure.FileStorage;

public class MinioFileStorageService(
        IMinioClient minio,
        IOptions<FileStorageSettings> settings,
        IArchivoRepository archivoRepository,
        ICurrentUserProvider currentUser) : IFileStorageService
{
    private bool _bucketEnsured;

    public async Task<Result<ArchivoResponse>> SubirAsync(
        Stream contenido, string nombreArchivo, string contentType,
        CancellationToken ct = default)
    {
        if (contenido.Length > settings.Value.MaxFileSize)
            return Result<ArchivoResponse>.Failure(
                $"El archivo excede el tamaño máximo permitido ({settings.Value.MaxFileSize / 1_048_576} MB).",
                ErrorType.Validation);

        var (esValido, contentTypeValidado, errorValidacion) = FileTypeValidator.Validar(nombreArchivo, contenido);
        if (!esValido)
            return Result<ArchivoResponse>.Failure(errorValidacion!, ErrorType.Validation);

        contentType = contentTypeValidado!;

        await EnsureBucketAsync(ct);

        var archivoId = Guid.NewGuid();
        var extension = Path.GetExtension(nombreArchivo);
        var objectName = BuildObjectName(archivoId, extension);

        // Subir a MinIO
        contenido.Position = 0;
        await minio.PutObjectAsync(new PutObjectArgs()
            .WithBucket(settings.Value.BucketName)
            .WithObject(objectName)
            .WithStreamData(contenido)
            .WithObjectSize(contenido.Length)
            .WithContentType(contentType), ct);

        // Crear registro en BD (sin SaveChanges — el controller guarda atómicamente)
        var archivo = new Archivo
        {
            Id = archivoId,
            NombreOriginal = nombreArchivo,
            ContentType = contentType,
            Tamaño = contenido.Length,
            EsThumbnail = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser.Current?.Id
        };

        await archivoRepository.CreateAsync(archivo);

        // Generar thumbnail si es imagen
        if (EsImagen(contentType))
        {
            await GenerarThumbnailAsync(contenido, archivo, ct);
        }

        return Result<ArchivoResponse>.Success(archivo.ToResponse());
    }

    public async Task<Result<ArchivoDescarga>> DescargarAsync(Guid archivoId, CancellationToken ct = default)
    {
        var archivo = await archivoRepository.GetByIdAsync(archivoId);

        if (archivo is null)
            return Result<ArchivoDescarga>.Failure("Archivo no encontrado.", ErrorType.NotFound);

        var extension = Path.GetExtension(archivo.NombreOriginal);
        var objectName = BuildObjectName(archivo.Id, extension);

        return Result<ArchivoDescarga>.Success(
            new ArchivoDescarga(
                EscribirContenido: (destino, token) => StreamFromMinioAsync(objectName, destino, token),
                ContentType: archivo.ContentType,
                NombreArchivo: archivo.NombreOriginal));
    }

    public async Task<Result<ArchivoDescarga>> DescargarThumbnailAsync(Guid archivoId, CancellationToken ct = default)
    {
        var archivo = await archivoRepository.GetByIdConThumbnailsAsync(archivoId);

        if (archivo is null)
            return Result<ArchivoDescarga>.Failure("Archivo no encontrado.", ErrorType.NotFound);

        var thumbnail = archivo.Thumbnails.FirstOrDefault();

        if (thumbnail is null)
            return Result<ArchivoDescarga>.Failure("Este archivo no tiene thumbnail.", ErrorType.NotFound);

        var objectName = BuildObjectName(thumbnail.Id, ".webp");

        return Result<ArchivoDescarga>.Success(
            new ArchivoDescarga(
                EscribirContenido: (destino, token) => StreamFromMinioAsync(objectName, destino, token),
                ContentType: thumbnail.ContentType,
                NombreArchivo: $"thumb_{Path.GetFileNameWithoutExtension(archivo.NombreOriginal)}.webp"));
    }

    public async Task<Result<ArchivoResponse>> ObtenerMetadataAsync(Guid archivoId, CancellationToken ct = default)
    {
        var archivo = await archivoRepository.GetByIdAsync(archivoId);

        if (archivo is null)
            return Result<ArchivoResponse>.Failure("Archivo no encontrado.", ErrorType.NotFound);

        return Result<ArchivoResponse>.Success(archivo.ToResponse());
    }

    public async Task<Result> EliminarAsync(Guid archivoId, CancellationToken ct = default)
    {
        var archivo = await archivoRepository.GetByIdConThumbnailsAsync(archivoId);

        if (archivo is null)
            return Result.Failure("Archivo no encontrado.", ErrorType.NotFound);

        var extension = Path.GetExtension(archivo.NombreOriginal);
        var ahora = DateTime.UtcNow;
        var usuarioId = currentUser.Current?.Id;

        // Eliminar thumbnails de MinIO y marcar como eliminados
        foreach (var thumbnail in archivo.Thumbnails)
        {
            var thumbObjectName = BuildObjectName(thumbnail.Id, ".webp");
            await RemoveObjectSafeAsync(thumbObjectName, ct);
            thumbnail.IsDeleted = true;
            thumbnail.DeletedAt = ahora;
            thumbnail.DeletedBy = usuarioId;
        }

        // Eliminar archivo original de MinIO y marcar como eliminado
        var objectName = BuildObjectName(archivo.Id, extension);
        await RemoveObjectSafeAsync(objectName, ct);
        archivo.IsDeleted = true;
        archivo.DeletedAt = ahora;
        archivo.DeletedBy = usuarioId;

        // Sin SaveChanges — el controller guarda atómicamente

        return Result.Success();
    }

    private async Task GenerarThumbnailAsync(Stream contenido, Archivo archivoOriginal, CancellationToken ct)
    {
        contenido.Position = 0;

        using var image = await Image.LoadAsync(contenido, ct);

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(settings.Value.ThumbnailSize, settings.Value.ThumbnailSize),
            Mode = ResizeMode.Max
        }));

        using var thumbnailStream = new MemoryStream();
        await image.SaveAsWebpAsync(thumbnailStream, ct);
        thumbnailStream.Position = 0;

        var thumbnailId = Guid.NewGuid();
        var thumbObjectName = BuildObjectName(thumbnailId, ".webp");

        await minio.PutObjectAsync(new PutObjectArgs()
            .WithBucket(settings.Value.BucketName)
            .WithObject(thumbObjectName)
            .WithStreamData(thumbnailStream)
            .WithObjectSize(thumbnailStream.Length)
            .WithContentType("image/webp"), ct);

        var thumbnail = new Archivo
        {
            Id = thumbnailId,
            NombreOriginal = $"thumb_{Path.GetFileNameWithoutExtension(archivoOriginal.NombreOriginal)}.webp",
            ContentType = "image/webp",
            Tamaño = thumbnailStream.Length,
            EsThumbnail = true,
            ArchivoOriginalId = archivoOriginal.Id,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser.Current?.Id
        };

        await archivoRepository.CreateAsync(thumbnail);
    }

    private async Task StreamFromMinioAsync(string objectName, Stream destino, CancellationToken ct)
    {
        await minio.GetObjectAsync(new GetObjectArgs()
            .WithBucket(settings.Value.BucketName)
            .WithObject(objectName)
            .WithCallbackStream(async (src, token) => await src.CopyToAsync(destino, token)), ct);
    }

    private async Task EnsureBucketAsync(CancellationToken ct)
    {
        if (_bucketEnsured) return;

        var exists = await minio.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(settings.Value.BucketName), ct);

        if (!exists)
        {
            await minio.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(settings.Value.BucketName), ct);
        }

        _bucketEnsured = true;
    }

    private async Task RemoveObjectSafeAsync(string objectName, CancellationToken ct)
    {
        try
        {
            await minio.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(settings.Value.BucketName)
                .WithObject(objectName), ct);
        }
        catch (ObjectNotFoundException) { }
    }

    private static string BuildObjectName(Guid archivoId, string extension)
    {
        return $"{archivoId}{extension}";
    }

    private static bool EsImagen(string contentType)
    {
        return contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }
}
