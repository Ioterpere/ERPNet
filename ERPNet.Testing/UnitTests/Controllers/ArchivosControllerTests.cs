using ERPNet.Api.Controllers;
using ERPNet.Application.Common;
using ERPNet.Contracts;
using ERPNet.Application.FileStorage;
using ERPNet.Contracts.FileStorage;
using ERPNet.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.UnitTests.Controllers;

public class ArchivosControllerTests
{
    private readonly IFileStorageService _fileStorage = Substitute.For<IFileStorageService>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ArchivosController _sut;

    public ArchivosControllerTests()
    {
        _sut = new ArchivosController(_fileStorage, _uow);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    private static IFormFile CrearFormFile(string nombre = "doc.pdf", string contentType = "application/pdf", int length = 100)
    {
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns(nombre);
        file.ContentType.Returns(contentType);
        file.Length.Returns(length);
        file.OpenReadStream().Returns(new MemoryStream(new byte[length]));
        return file;
    }

    [Fact(DisplayName = "Subir: exitoso devuelve 201")]
    public async Task Subir_Exitoso_Devuelve201()
    {
        var archivoResponse = new ArchivoResponse
        {
            Id = Guid.NewGuid(), NombreOriginal = "doc.pdf", ContentType = "application/pdf"
        };
        _fileStorage.SubirAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<ArchivoResponse>.Success(archivoResponse));

        var result = await _sut.Subir(CrearFormFile(), CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, objectResult.StatusCode);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Subir: archivo vacio devuelve BadRequest")]
    public async Task Subir_ArchivoVacio_DevuelveBadRequest()
    {
        var result = await _sut.Subir(CrearFormFile(length: 0), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact(DisplayName = "Subir: error del storage no guarda cambios")]
    public async Task Subir_ErrorStorage_NoGuardaCambios()
    {
        _fileStorage.SubirAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<ArchivoResponse>.Failure("Error", ErrorType.InternalError));

        var result = await _sut.Subir(CrearFormFile(), CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Eliminar: exitoso devuelve NoContent")]
    public async Task Eliminar_Exitoso_DevuelveNoContent()
    {
        var id = Guid.NewGuid();
        _fileStorage.EliminarAsync(id, Arg.Any<CancellationToken>()).Returns(Result.Success());

        var result = await _sut.Eliminar(id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Eliminar: error devuelve status code sin guardar")]
    public async Task Eliminar_Error_DevuelveStatusCode()
    {
        var id = Guid.NewGuid();
        _fileStorage.EliminarAsync(id, Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No encontrado", ErrorType.NotFound));

        var result = await _sut.Eliminar(id, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
