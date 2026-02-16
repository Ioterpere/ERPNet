using ERPNet.Api.Controllers;
using ERPNet.Application.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Enums;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Application.FileStorage;
using ERPNet.Application.Reports.DTOs;
using ERPNet.Application.Reports.Interfaces;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.UnitTests.Controllers;

public class EmpleadosControllerTests
{
    private readonly IEmpleadoService _service = Substitute.For<IEmpleadoService>();
    private readonly IReporteEmpleadoService _reporteService = Substitute.For<IReporteEmpleadoService>();
    private readonly IFileStorageService _fileStorage = Substitute.For<IFileStorageService>();
    private readonly IEmpleadoRepository _repo = Substitute.For<IEmpleadoRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly EmpleadosController _sut;

    public EmpleadosControllerTests()
    {
        _sut = new EmpleadosController(_fileStorage, _repo, _uow, _reporteService, _service);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    #region CRUD

    [Fact(DisplayName = "GetAll: exitoso devuelve 200")]
    public async Task GetAll_Exitoso_Devuelve200()
    {
        var lista = new ListaPaginada<EmpleadoResponse>
        {
            Items = [new EmpleadoResponse { Id = 1, Nombre = "Juan", Apellidos = "G", Dni = "12345678Z" }],
            Pagina = 1, PorPagina = 50, TotalRegistros = 1
        };
        _service.GetAllAsync(Arg.Any<PaginacionFilter>())
            .Returns(Result<ListaPaginada<EmpleadoResponse>>.Success(lista));

        var result = await _sut.GetAll(new PaginacionFilter());

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<ListaPaginada<EmpleadoResponse>>(okResult.Value);
    }

    [Fact(DisplayName = "GetById: existente devuelve 200")]
    public async Task GetById_Existente_Devuelve200()
    {
        var response = new EmpleadoResponse { Id = 1, Nombre = "Juan", Apellidos = "G", Dni = "12345678Z" };
        _service.GetByIdAsync(1).Returns(Result<EmpleadoResponse>.Success(response));

        var result = await _sut.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, ((EmpleadoResponse)okResult.Value!).Id);
    }

    [Fact(DisplayName = "GetById: inexistente devuelve 404")]
    public async Task GetById_Inexistente_Devuelve404()
    {
        _service.GetByIdAsync(99)
            .Returns(Result<EmpleadoResponse>.Failure("No encontrado", ErrorType.NotFound));

        var result = await _sut.GetById(99);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact(DisplayName = "GetMe: exitoso devuelve 200")]
    public async Task GetMe_Exitoso_Devuelve200()
    {
        var response = new EmpleadoResponse { Id = 1, Nombre = "Juan", Apellidos = "G", Dni = "12345678Z" };
        _service.GetMeAsync().Returns(Result<EmpleadoResponse>.Success(response));

        var result = await _sut.GetMe();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact(DisplayName = "Create: exitoso devuelve 201")]
    public async Task Create_Exitoso_Devuelve201()
    {
        var response = new EmpleadoResponse { Id = 1, Nombre = "Ana", Apellidos = "L", Dni = "87654321X" };
        _service.CreateAsync(Arg.Any<CreateEmpleadoRequest>())
            .Returns(Result<EmpleadoResponse>.Success(response));

        var result = await _sut.Create(new CreateEmpleadoRequest
        {
            Nombre = "Ana", Apellidos = "L", Dni = "87654321X", SeccionId = 1
        });

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, objectResult.StatusCode);
    }

    [Fact(DisplayName = "Update: exitoso devuelve 204")]
    public async Task Update_Exitoso_Devuelve204()
    {
        _service.UpdateAsync(1, Arg.Any<UpdateEmpleadoRequest>()).Returns(Result.Success());

        var result = await _sut.Update(1, new UpdateEmpleadoRequest { Nombre = "Pedro" });

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

    #region Reporte

    [Fact(DisplayName = "Reporte: exitoso devuelve FileContentResult")]
    public async Task Reporte_Exitoso_DevuelveFileResult()
    {
        var archivo = new ReporteArchivo([0x25, 0x50, 0x44, 0x46], "application/pdf", "empleados.pdf");
        _reporteService.GenerarAsync(Arg.Any<EmpleadoReporteFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result<ReporteArchivo>.Success(archivo));

        var result = await _sut.Reporte(new EmpleadoReporteFilter());

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal("empleados.pdf", fileResult.FileDownloadName);
    }

    [Fact(DisplayName = "Reporte: error devuelve status code")]
    public async Task Reporte_Error_DevuelveStatusCode()
    {
        _reporteService.GenerarAsync(Arg.Any<EmpleadoReporteFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result<ReporteArchivo>.Failure("Error interno", ErrorType.InternalError));

        var result = await _sut.Reporte(new EmpleadoReporteFilter());

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    #endregion
}
