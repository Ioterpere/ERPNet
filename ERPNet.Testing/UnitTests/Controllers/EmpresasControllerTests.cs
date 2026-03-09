using ERPNet.Api.Controllers;
using ERPNet.Application.Auth;
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

public class EmpresasControllerTests
{
    private readonly IEmpresaService _service = Substitute.For<IEmpresaService>();
    private readonly EmpresasController _sut;

    public EmpresasControllerTests()
    {
        _sut = new EmpresasController(_service);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _sut.HttpContext.Items["UsuarioContext"] = new UsuarioContext
        {
            Id = 1, Email = "admin@erpnet.com", EmpleadoId = 1, SeccionId = 1, RolIds = [1]
        };
    }

    [Fact(DisplayName = "GetAll: exitoso devuelve 200")]
    public async Task GetAll_Exitoso_Devuelve200()
    {
        var paginado = ListaPaginada<EmpresaResponse>.Crear([], 0);
        _service.GetAllAsync(Arg.Any<PaginacionFilter>())
            .Returns(Result<ListaPaginada<EmpresaResponse>>.Success(paginado));

        var result = await _sut.GetAll(new PaginacionFilter());

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact(DisplayName = "GetById: inexistente devuelve 404")]
    public async Task GetById_Inexistente_Devuelve404()
    {
        _service.GetByIdAsync(99)
            .Returns(Result<EmpresaResponse>.Failure("No encontrada.", ErrorType.NotFound));

        var result = await _sut.GetById(99);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact(DisplayName = "Create: exitoso devuelve 201")]
    public async Task Create_Exitoso_Devuelve201()
    {
        var empresa = new EmpresaResponse { Id = 1, Nombre = "Nueva SA", Activo = true };
        _service.CreateAsync(Arg.Any<CreateEmpresaRequest>())
            .Returns(Result<EmpresaResponse>.Success(empresa));

        var result = await _sut.Create(new CreateEmpresaRequest { Nombre = "Nueva SA" });

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, objectResult.StatusCode);
    }

    [Fact(DisplayName = "Delete: exitoso devuelve 204")]
    public async Task Delete_Exitoso_Devuelve204()
    {
        _service.DeleteAsync(1).Returns(Result.Success());

        var result = await _sut.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }
}
