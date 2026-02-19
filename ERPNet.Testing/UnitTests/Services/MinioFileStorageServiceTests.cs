using ERPNet.Application;
using ERPNet.Application.Auth;
using ERPNet.Application.Auth.Interfaces;
using ERPNet.Contracts;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Repositories;
using ERPNet.Infrastructure.FileStorage;
using Microsoft.Extensions.Options;
using Minio;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.UnitTests.Services;

public class MinioFileStorageServiceTests
{
    private readonly IMinioClient _minio = Substitute.For<IMinioClient>();
    private readonly IArchivoRepository _archivoRepo = Substitute.For<IArchivoRepository>();
    private readonly ICurrentUserProvider _currentUser = Substitute.For<ICurrentUserProvider>();

    private readonly MinioFileStorageService _sut;

    private readonly FileStorageSettings _settings = new()
    {
        Endpoint = "localhost:9000",
        AccessKey = "test",
        SecretKey = "test",
        BucketName = "test-bucket",
        UseSSL = false,
        ThumbnailSize = 300,
        MaxFileSize = 20_971_520 // 20 MB
    };

    public MinioFileStorageServiceTests()
    {
        _currentUser.Current.Returns(new UsuarioContext(1, "test@test.com", 1, 1, [], [], false));

        _sut = new MinioFileStorageService(
            _minio,
            Options.Create(_settings),
            _archivoRepo,
            _currentUser);
    }

    private static MemoryStream CrearStreamPng(long? tamaño = null)
    {
        // PNG magic bytes + suficiente padding
        byte[] pngBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x00];

        if (tamaño is null || tamaño <= pngBytes.Length)
            return new MemoryStream(pngBytes);

        // Crear stream del tamaño solicitado con los magic bytes al inicio
        var buffer = new byte[tamaño.Value];
        Array.Copy(pngBytes, buffer, pngBytes.Length);
        return new MemoryStream(buffer);
    }

    private static MemoryStream CrearStreamTxt() => new([0x48, 0x6F, 0x6C, 0x61]); // "Hola"

    #region SubirAsync - Validaciones

    [Fact(DisplayName = "Subir: archivo que excede MaxFileSize devuelve Validation")]
    public async Task Subir_ExcedeMaxFileSize_DevuelveValidation()
    {
        using var stream = CrearStreamPng(_settings.MaxFileSize + 1);

        var result = await _sut.SubirAsync(stream, "grande.png", "image/png");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.Contains("tamaño máximo", result.Error!);
    }

    [Fact(DisplayName = "Subir: extensión no permitida (.exe) devuelve Validation")]
    public async Task Subir_ExtensionNoPermitida_DevuelveValidation()
    {
        using var stream = new MemoryStream([0x4D, 0x5A, 0x90, 0x00]);

        var result = await _sut.SubirAsync(stream, "app.exe", "application/octet-stream");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.Contains(".exe", result.Error!);
    }

    [Fact(DisplayName = "Subir: magic bytes incorrectos (.png con bytes .jpg) devuelve Validation")]
    public async Task Subir_MagicBytesIncorrectos_DevuelveValidation()
    {
        // Bytes de JPG con extensión PNG
        using var stream = new MemoryStream([0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x00, 0x00, 0x00]);

        var result = await _sut.SubirAsync(stream, "fake.png", "image/png");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.Contains(".png", result.Error!);
    }

    #endregion

    #region SubirAsync - Caso exitoso

    [Fact(DisplayName = "Subir: archivo válido crea registro y sube a MinIO")]
    public async Task Subir_ArchivoValido_GuardaCorrectamente()
    {
        using var stream = CrearStreamTxt();

        var result = await _sut.SubirAsync(stream, "datos.txt", "application/octet-stream");

        Assert.True(result.IsSuccess);
        Assert.Equal("text/plain", result.Value!.ContentType);
        await _archivoRepo.Received().CreateAsync(Arg.Is<Archivo>(a => a.ContentType == "text/plain"));
    }

    #endregion

    #region DescargarAsync

    [Fact(DisplayName = "Descargar: archivo inexistente devuelve NotFound")]
    public async Task Descargar_ArchivoInexistente_DevuelveNotFound()
    {
        _archivoRepo.GetByIdAsync(Arg.Any<Guid>()).Returns((Archivo?)null);

        var result = await _sut.DescargarAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion

    #region DescargarThumbnailAsync

    [Fact(DisplayName = "DescargarThumbnail: archivo inexistente devuelve NotFound")]
    public async Task DescargarThumbnail_ArchivoInexistente_DevuelveNotFound()
    {
        _archivoRepo.GetByIdConThumbnailsAsync(Arg.Any<Guid>()).Returns((Archivo?)null);

        var result = await _sut.DescargarThumbnailAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact(DisplayName = "DescargarThumbnail: sin thumbnail devuelve NotFound")]
    public async Task DescargarThumbnail_SinThumbnail_DevuelveNotFound()
    {
        var archivo = new Archivo
        {
            Id = Guid.NewGuid(),
            NombreOriginal = "foto.png",
            ContentType = "image/png",
            Tamaño = 1000,
            Thumbnails = []
        };
        _archivoRepo.GetByIdConThumbnailsAsync(archivo.Id).Returns(archivo);

        var result = await _sut.DescargarThumbnailAsync(archivo.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.Contains("thumbnail", result.Error!);
    }

    #endregion

    #region ObtenerMetadataAsync

    [Fact(DisplayName = "ObtenerMetadata: archivo inexistente devuelve NotFound")]
    public async Task ObtenerMetadata_ArchivoInexistente_DevuelveNotFound()
    {
        _archivoRepo.GetByIdAsync(Arg.Any<Guid>()).Returns((Archivo?)null);

        var result = await _sut.ObtenerMetadataAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact(DisplayName = "ObtenerMetadata: archivo existente devuelve ArchivoResponse")]
    public async Task ObtenerMetadata_ArchivoExistente_DevuelveResponse()
    {
        var archivo = new Archivo
        {
            Id = Guid.NewGuid(),
            NombreOriginal = "doc.pdf",
            ContentType = "application/pdf",
            Tamaño = 5000,
            CreatedAt = DateTime.UtcNow
        };
        _archivoRepo.GetByIdAsync(archivo.Id).Returns(archivo);

        var result = await _sut.ObtenerMetadataAsync(archivo.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(archivo.Id, result.Value!.Id);
        Assert.Equal("doc.pdf", result.Value.NombreOriginal);
        Assert.Equal("application/pdf", result.Value.ContentType);
    }

    #endregion

    #region EliminarAsync

    [Fact(DisplayName = "Eliminar: archivo inexistente devuelve NotFound")]
    public async Task Eliminar_ArchivoInexistente_DevuelveNotFound()
    {
        _archivoRepo.GetByIdConThumbnailsAsync(Arg.Any<Guid>()).Returns((Archivo?)null);

        var result = await _sut.EliminarAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact(DisplayName = "Eliminar: marca soft delete sin llamar SaveChanges")]
    public async Task Eliminar_ArchivoExistente_MarcaSoftDeleteSinSaveChanges()
    {
        var archivo = new Archivo
        {
            Id = Guid.NewGuid(),
            NombreOriginal = "foto.png",
            ContentType = "image/png",
            Tamaño = 1000,
            Thumbnails = []
        };
        _archivoRepo.GetByIdConThumbnailsAsync(archivo.Id).Returns(archivo);

        var result = await _sut.EliminarAsync(archivo.Id);

        Assert.True(result.IsSuccess);
        Assert.True(archivo.IsDeleted);
        Assert.NotNull(archivo.DeletedAt);
        Assert.Equal(1, archivo.DeletedBy);
    }

    [Fact(DisplayName = "Eliminar: también marca thumbnails como eliminados")]
    public async Task Eliminar_ConThumbnails_MarcaTodosSoftDelete()
    {
        var thumbnail = new Archivo
        {
            Id = Guid.NewGuid(),
            NombreOriginal = "thumb_foto.webp",
            ContentType = "image/webp",
            Tamaño = 500,
            EsThumbnail = true
        };
        var archivo = new Archivo
        {
            Id = Guid.NewGuid(),
            NombreOriginal = "foto.png",
            ContentType = "image/png",
            Tamaño = 1000,
            Thumbnails = [thumbnail]
        };
        _archivoRepo.GetByIdConThumbnailsAsync(archivo.Id).Returns(archivo);

        var result = await _sut.EliminarAsync(archivo.Id);

        Assert.True(result.IsSuccess);
        Assert.True(archivo.IsDeleted);
        Assert.True(thumbnail.IsDeleted);
        Assert.NotNull(thumbnail.DeletedAt);
        Assert.Equal(1, thumbnail.DeletedBy);
    }

    #endregion
}
