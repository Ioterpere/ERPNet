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

public class PlanCuentasControllerTests
{
    private readonly ICuentaService _service = Substitute.For<ICuentaService>();
    private readonly PlanCuentasController _sut;

    public PlanCuentasControllerTests()
    {
        _sut = new PlanCuentasController(_service);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    #region CRUD

    [Fact(DisplayName = "GetAll: exitoso devuelve 200")]
    public async Task GetAll_Exitoso_Devuelve200()
    {
        var lista = new ListaPaginada<CuentaResponse>
        {
            Items = [new CuentaResponse { Id = 1, Codigo = "10000000", Descripcion = "Test" }],
            TotalRegistros = 1
        };
        _service.GetAllAsync(Arg.Any<CuentaFilter>())
            .Returns(Result<ListaPaginada<CuentaResponse>>.Success(lista));

        var result = await _sut.GetAll(new CuentaFilter());

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<ListaPaginada<CuentaResponse>>(okResult.Value);
    }

    [Fact(DisplayName = "GetById: existente devuelve 200")]
    public async Task GetById_Existente_Devuelve200()
    {
        var response = new CuentaResponse { Id = 1, Codigo = "10000000", Descripcion = "Test" };
        _service.GetByIdAsync(1).Returns(Result<CuentaResponse>.Success(response));

        var result = await _sut.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, ((CuentaResponse)okResult.Value!).Id);
    }

    [Fact(DisplayName = "GetById: inexistente devuelve 404")]
    public async Task GetById_Inexistente_Devuelve404()
    {
        _service.GetByIdAsync(99)
            .Returns(Result<CuentaResponse>.Failure("No encontrada", ErrorType.NotFound));

        var result = await _sut.GetById(99);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact(DisplayName = "Create: exitoso devuelve 201")]
    public async Task Create_Exitoso_Devuelve201()
    {
        var response = new CuentaResponse { Id = 1, Codigo = "10000000", Descripcion = "Test" };
        _service.CreateAsync(Arg.Any<CreateCuentaRequest>())
            .Returns(Result<CuentaResponse>.Success(response));

        var result = await _sut.Create(new CreateCuentaRequest { Codigo = "10000000", Descripcion = "Test" });

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, objectResult.StatusCode);
    }

    [Fact(DisplayName = "Create: código duplicado devuelve 409")]
    public async Task Create_CodigoDuplicado_Devuelve409()
    {
        _service.CreateAsync(Arg.Any<CreateCuentaRequest>())
            .Returns(Result<CuentaResponse>.Failure("Código duplicado", ErrorType.Conflict));

        var result = await _sut.Create(new CreateCuentaRequest { Codigo = "10000000", Descripcion = "X" });

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(409, objectResult.StatusCode);
    }

    [Fact(DisplayName = "Update: exitoso devuelve 204")]
    public async Task Update_Exitoso_Devuelve204()
    {
        _service.UpdateAsync(1, Arg.Any<UpdateCuentaRequest>()).Returns(Result.Success());

        var result = await _sut.Update(1, new UpdateCuentaRequest { Descripcion = "Nueva" });

        Assert.IsType<NoContentResult>(result);
    }

    [Fact(DisplayName = "Update: inexistente devuelve 404")]
    public async Task Update_Inexistente_Devuelve404()
    {
        _service.UpdateAsync(99, Arg.Any<UpdateCuentaRequest>())
            .Returns(Result.Failure("No encontrada", ErrorType.NotFound));

        var result = await _sut.Update(99, new UpdateCuentaRequest { Descripcion = "X" });

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
        _service.DeleteAsync(99).Returns(Result.Failure("No encontrada", ErrorType.NotFound));

        var result = await _sut.Delete(99);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    #endregion

    #region Extracto y Saldos

    [Fact(DisplayName = "GetExtracto: exitoso devuelve 200")]
    public async Task GetExtracto_Exitoso_Devuelve200()
    {
        _service.GetExtractoAsync(1, Arg.Any<ExtractoFilter>())
            .Returns(Result<List<ApunteContableResponse>>.Success([]));

        var result = await _sut.GetExtracto(1, new ExtractoFilter());

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact(DisplayName = "GetExtracto: cuenta inexistente devuelve 404")]
    public async Task GetExtracto_Inexistente_Devuelve404()
    {
        _service.GetExtractoAsync(99, Arg.Any<ExtractoFilter>())
            .Returns(Result<List<ApunteContableResponse>>.Failure("No encontrada", ErrorType.NotFound));

        var result = await _sut.GetExtracto(99, new ExtractoFilter());

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact(DisplayName = "GetSaldos: exitoso devuelve 200")]
    public async Task GetSaldos_Exitoso_Devuelve200()
    {
        _service.GetSaldosAsync(1, 2026)
            .Returns(Result<List<SaldoMensualResponse>>.Success([]));

        var result = await _sut.GetSaldos(1, 2026);

        Assert.IsType<OkObjectResult>(result);
    }

    #endregion

    #region Catálogos

    [Fact(DisplayName = "GetTiposDiario: exitoso devuelve 200")]
    public async Task GetTiposDiario_Exitoso_Devuelve200()
    {
        _service.GetTiposDiarioAsync()
            .Returns(Result<List<TipoDiarioResponse>>.Success([]));

        var result = await _sut.GetTiposDiario();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact(DisplayName = "GetCentrosCoste: exitoso devuelve 200")]
    public async Task GetCentrosCoste_Exitoso_Devuelve200()
    {
        _service.GetCentrosCostosAsync()
            .Returns(Result<List<CentroCosteResponse>>.Success([]));

        var result = await _sut.GetCentrosCoste();

        Assert.IsType<OkObjectResult>(result);
    }

    #endregion
}
