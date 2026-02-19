using ERPNet.Api.Controllers;
using ERPNet.Application.Common;
using ERPNet.Contracts.DTOs;
using ERPNet.Contracts;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Application.FileStorage;
using ERPNet.Contracts.FileStorage;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.UnitTests.Controllers;

public class MaquinasControllerTests
{
    private readonly IMaquinariaService _service = Substitute.For<IMaquinariaService>();
    private readonly IFileStorageService _fileStorage = Substitute.For<IFileStorageService>();
    private readonly IMaquinariaRepository _repo = Substitute.For<IMaquinariaRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly MaquinasController _sut;

    public MaquinasControllerTests()
    {
        _sut = new MaquinasController(_fileStorage, _repo, _uow, _service);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    private static IFormFile CrearFormFile(string nombre = "foto.jpg", string contentType = "image/jpeg", int length = 100)
    {
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns(nombre);
        file.ContentType.Returns(contentType);
        file.Length.Returns(length);
        file.OpenReadStream().Returns(new MemoryStream(new byte[length]));
        return file;
    }

    private static Maquinaria CrearMaquinaria(int id = 1, Guid? fotoId = null) => new()
    {
        Id = id,
        Nombre = "Torno CNC",
        Codigo = "T-001",
        Activa = true,
        FotoId = fotoId
    };

    #region CRUD

    [Fact(DisplayName = "GetAll: exitoso devuelve 200")]
    public async Task GetAll_Exitoso_Devuelve200()
    {
        var lista = new ListaPaginada<MaquinariaResponse>
        {
            Items = [new MaquinariaResponse { Id = 1, Nombre = "Torno", Codigo = "T-001" }],
            Pagina = 1, PorPagina = 50, TotalRegistros = 1
        };
        _service.GetAllAsync(Arg.Any<PaginacionFilter>())
            .Returns(Result<ListaPaginada<MaquinariaResponse>>.Success(lista));

        var result = await _sut.GetAll(new PaginacionFilter());

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact(DisplayName = "GetById: existente devuelve 200")]
    public async Task GetById_Existente_Devuelve200()
    {
        var response = new MaquinariaResponse { Id = 1, Nombre = "Torno", Codigo = "T-001" };
        _service.GetByIdAsync(1).Returns(Result<MaquinariaResponse>.Success(response));

        var result = await _sut.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, ((MaquinariaResponse)okResult.Value!).Id);
    }

    [Fact(DisplayName = "GetById: inexistente devuelve 404")]
    public async Task GetById_Inexistente_Devuelve404()
    {
        _service.GetByIdAsync(99)
            .Returns(Result<MaquinariaResponse>.Failure("No encontrado", ErrorType.NotFound));

        var result = await _sut.GetById(99);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact(DisplayName = "Create: exitoso devuelve 201")]
    public async Task Create_Exitoso_Devuelve201()
    {
        var response = new MaquinariaResponse { Id = 1, Nombre = "Nueva", Codigo = "N-001" };
        _service.CreateAsync(Arg.Any<CreateMaquinariaRequest>())
            .Returns(Result<MaquinariaResponse>.Success(response));

        var result = await _sut.Create(new CreateMaquinariaRequest { Nombre = "Nueva", Codigo = "N-001" });

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, objectResult.StatusCode);
    }

    [Fact(DisplayName = "Update: exitoso devuelve 204")]
    public async Task Update_Exitoso_Devuelve204()
    {
        _service.UpdateAsync(1, Arg.Any<UpdateMaquinariaRequest>()).Returns(Result.Success());

        var result = await _sut.Update(1, new UpdateMaquinariaRequest { Nombre = "Editada" });

        Assert.IsType<NoContentResult>(result);
    }

    [Fact(DisplayName = "Delete: exitoso devuelve 204")]
    public async Task Delete_Exitoso_Devuelve204()
    {
        _service.DeleteAsync(1).Returns(Result.Success());

        var result = await _sut.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    #endregion

    #region ArchivoBaseController - SubirArchivo

    [Fact(DisplayName = "SubirArchivo: exitoso devuelve Ok con ArchivoResponse")]
    public async Task SubirArchivo_Exitoso_DevuelveOk()
    {
        var maquinaria = CrearMaquinaria();
        _repo.GetByIdAsync(1).Returns(maquinaria);
        var archivoResponse = new ArchivoResponse { Id = Guid.NewGuid(), NombreOriginal = "foto.jpg", ContentType = "image/jpeg" };
        _fileStorage.SubirAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<ArchivoResponse>.Success(archivoResponse));

        var result = await _sut.SubirArchivo(1, "foto", CrearFormFile(), CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<ArchivoResponse>(okResult.Value);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "SubirArchivo: campo invalido devuelve NotFound")]
    public async Task SubirArchivo_CampoInvalido_DevuelveNotFound()
    {
        var result = await _sut.SubirArchivo(1, "campo-inexistente", CrearFormFile(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact(DisplayName = "SubirArchivo: archivo vacio devuelve BadRequest")]
    public async Task SubirArchivo_ArchivoVacio_DevuelveBadRequest()
    {
        var result = await _sut.SubirArchivo(1, "foto", CrearFormFile(length: 0), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact(DisplayName = "SubirArchivo: entidad inexistente devuelve NotFound")]
    public async Task SubirArchivo_EntidadInexistente_DevuelveNotFound()
    {
        _repo.GetByIdAsync(99).Returns((Maquinaria?)null);

        var result = await _sut.SubirArchivo(99, "foto", CrearFormFile(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region ArchivoBaseController - EliminarArchivo

    [Fact(DisplayName = "EliminarArchivo: exitoso devuelve NoContent")]
    public async Task EliminarArchivo_Exitoso_DevuelveNoContent()
    {
        var fotoId = Guid.NewGuid();
        var maquinaria = CrearMaquinaria(fotoId: fotoId);
        _repo.GetByIdAsync(1).Returns(maquinaria);
        _fileStorage.EliminarAsync(fotoId, Arg.Any<CancellationToken>()).Returns(Result.Success());

        var result = await _sut.EliminarArchivo(1, "foto", CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(maquinaria.FotoId);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "EliminarArchivo: slot vacio devuelve NotFound")]
    public async Task EliminarArchivo_SlotVacio_DevuelveNotFound()
    {
        var maquinaria = CrearMaquinaria(fotoId: null);
        _repo.GetByIdAsync(1).Returns(maquinaria);

        var result = await _sut.EliminarArchivo(1, "foto", CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    #endregion
}
