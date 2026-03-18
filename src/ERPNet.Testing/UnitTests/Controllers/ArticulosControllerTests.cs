using ERPNet.Api.Controllers;
using ERPNet.Application.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Enums;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.UnitTests.Controllers;

public class ArticulosControllerTests
{
    private readonly IArticuloService _service = Substitute.For<IArticuloService>();
    private readonly ArticulosController _sut;

    public ArticulosControllerTests()
    {
        _sut = new ArticulosController(_service);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    #region CRUD

    [Fact(DisplayName = "GetAll: exitoso devuelve 200")]
    public async Task GetAll_Exitoso_Devuelve200()
    {
        var lista = new ListaPaginada<ArticuloResponse>
        {
            Items = [new ArticuloResponse { Id = 1, Codigo = "ART-001", Descripcion = "Test" }],
            TotalRegistros = 1
        };
        _service.GetAllAsync(Arg.Any<PaginacionFilter>())
            .Returns(Result<ListaPaginada<ArticuloResponse>>.Success(lista));

        var result = await _sut.GetAll(new PaginacionFilter());

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<ListaPaginada<ArticuloResponse>>(okResult.Value);
    }

    [Fact(DisplayName = "GetById: existente devuelve 200")]
    public async Task GetById_Existente_Devuelve200()
    {
        var response = new ArticuloResponse { Id = 1, Codigo = "ART-001", Descripcion = "Test" };
        _service.GetByIdAsync(1).Returns(Result<ArticuloResponse>.Success(response));

        var result = await _sut.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, ((ArticuloResponse)okResult.Value!).Id);
    }

    [Fact(DisplayName = "GetById: inexistente devuelve 404")]
    public async Task GetById_Inexistente_Devuelve404()
    {
        _service.GetByIdAsync(99)
            .Returns(Result<ArticuloResponse>.Failure("No encontrado", ErrorType.NotFound));

        var result = await _sut.GetById(99);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact(DisplayName = "GetById: sin acceso devuelve 403")]
    public async Task GetById_SinAcceso_Devuelve403()
    {
        _service.GetByIdAsync(1)
            .Returns(Result<ArticuloResponse>.Failure("Prohibido", ErrorType.Forbidden));

        var result = await _sut.GetById(1);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact(DisplayName = "Create: exitoso devuelve 201")]
    public async Task Create_Exitoso_Devuelve201()
    {
        var response = new ArticuloResponse { Id = 1, Codigo = "ART-001", Descripcion = "Test" };
        _service.CreateAsync(Arg.Any<CreateArticuloRequest>())
            .Returns(Result<ArticuloResponse>.Success(response));

        var result = await _sut.Create(new CreateArticuloRequest { Codigo = "ART-001", Descripcion = "Test" });

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, objectResult.StatusCode);
    }

    [Fact(DisplayName = "Create: código duplicado devuelve 409")]
    public async Task Create_CodigoDuplicado_Devuelve409()
    {
        _service.CreateAsync(Arg.Any<CreateArticuloRequest>())
            .Returns(Result<ArticuloResponse>.Failure("Código duplicado", ErrorType.Conflict));

        var result = await _sut.Create(new CreateArticuloRequest { Codigo = "ART-001", Descripcion = "X" });

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(409, objectResult.StatusCode);
    }

    [Fact(DisplayName = "Update: exitoso devuelve 204")]
    public async Task Update_Exitoso_Devuelve204()
    {
        _service.UpdateAsync(1, Arg.Any<UpdateArticuloRequest>()).Returns(Result.Success());

        var result = await _sut.Update(1, new UpdateArticuloRequest { Descripcion = "Nueva" });

        Assert.IsType<NoContentResult>(result);
    }

    [Fact(DisplayName = "Update: inexistente devuelve 404")]
    public async Task Update_Inexistente_Devuelve404()
    {
        _service.UpdateAsync(99, Arg.Any<UpdateArticuloRequest>())
            .Returns(Result.Failure("No encontrado", ErrorType.NotFound));

        var result = await _sut.Update(99, new UpdateArticuloRequest { Descripcion = "X" });

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact(DisplayName = "Delete: exitoso devuelve 204")]
    public async Task Delete_Exitoso_Devuelve204()
    {
        _service.DeleteAsync(1).Returns(Result.Success());

        var result = await _sut.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact(DisplayName = "Delete: inexistente devuelve 404")]
    public async Task Delete_Inexistente_Devuelve404()
    {
        _service.DeleteAsync(99).Returns(Result.Failure("No encontrado", ErrorType.NotFound));

        var result = await _sut.Delete(99);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    #endregion

    #region Logs

    [Fact(DisplayName = "GetLogs: exitoso devuelve 200")]
    public async Task GetLogs_Exitoso_Devuelve200()
    {
        _service.GetLogsAsync(1)
            .Returns(Result<List<ArticuloLogResponse>>.Success([]));

        var result = await _sut.GetLogs(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact(DisplayName = "GetLogs: artículo inexistente devuelve 404")]
    public async Task GetLogs_Inexistente_Devuelve404()
    {
        _service.GetLogsAsync(99)
            .Returns(Result<List<ArticuloLogResponse>>.Failure("No encontrado", ErrorType.NotFound));

        var result = await _sut.GetLogs(99);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact(DisplayName = "AddLog: exitoso devuelve 201")]
    public async Task AddLog_Exitoso_Devuelve201()
    {
        var response = new ArticuloLogResponse
        {
            Id = 1, ArticuloId = 1, UsuarioId = 1,
            Fecha = DateOnly.FromDateTime(DateTime.Today),
            Nota = "Entrada", CreatedAt = DateTime.UtcNow
        };
        _service.AddLogAsync(1, Arg.Any<CreateArticuloLogRequest>())
            .Returns(Result<ArticuloLogResponse>.Success(response));

        var result = await _sut.AddLog(1, new CreateArticuloLogRequest
        {
            Fecha = DateOnly.FromDateTime(DateTime.Today),
            Nota = "Entrada"
        });

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, objectResult.StatusCode);
    }

    #endregion

    #region Catálogos

    [Fact(DisplayName = "GetFamilias: exitoso devuelve 200")]
    public async Task GetFamilias_Exitoso_Devuelve200()
    {
        _service.GetFamiliasAsync()
            .Returns(Result<List<FamiliaArticuloResponse>>.Success([]));

        var result = await _sut.GetFamilias();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact(DisplayName = "GetTiposIva: exitoso devuelve 200")]
    public async Task GetTiposIva_Exitoso_Devuelve200()
    {
        _service.GetTiposIvaAsync()
            .Returns(Result<List<TipoIvaResponse>>.Success([]));

        var result = await _sut.GetTiposIva();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact(DisplayName = "GetFormatos: exitoso devuelve 200")]
    public async Task GetFormatos_Exitoso_Devuelve200()
    {
        _service.GetFormatosAsync()
            .Returns(Result<List<FormatoArticuloResponse>>.Success([]));

        var result = await _sut.GetFormatos();

        Assert.IsType<OkObjectResult>(result);
    }

    #endregion
}
