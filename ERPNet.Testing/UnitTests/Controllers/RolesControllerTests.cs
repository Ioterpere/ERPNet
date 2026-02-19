using ERPNet.Api.Controllers;
using ERPNet.Application.Common;
using ERPNet.Contracts.DTOs;
using ERPNet.Contracts;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.UnitTests.Controllers;

public class RolesControllerTests
{
    private readonly IRolService _service = Substitute.For<IRolService>();
    private readonly RolesController _sut;

    public RolesControllerTests()
    {
        _sut = new RolesController(_service);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact(DisplayName = "GetAll: exitoso devuelve 200")]
    public async Task GetAll_Exitoso_Devuelve200()
    {
        var lista = new ListaPaginada<RolResponse>
        {
            Items = [new RolResponse { Id = 1, Nombre = "Admin" }],
            Pagina = 1, PorPagina = 50, TotalRegistros = 1
        };
        _service.GetAllAsync(Arg.Any<PaginacionFilter>())
            .Returns(Result<ListaPaginada<RolResponse>>.Success(lista));

        var result = await _sut.GetAll(new PaginacionFilter());

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<ListaPaginada<RolResponse>>(okResult.Value);
    }

    [Fact(DisplayName = "GetById: existente devuelve 200")]
    public async Task GetById_Existente_Devuelve200()
    {
        var response = new RolResponse { Id = 1, Nombre = "Admin" };
        _service.GetByIdAsync(1).Returns(Result<RolResponse>.Success(response));

        var result = await _sut.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Admin", ((RolResponse)okResult.Value!).Nombre);
    }

    [Fact(DisplayName = "GetById: inexistente devuelve 404")]
    public async Task GetById_Inexistente_Devuelve404()
    {
        _service.GetByIdAsync(99).Returns(Result<RolResponse>.Failure("No encontrado", ErrorType.NotFound));

        var result = await _sut.GetById(99);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact(DisplayName = "Create: exitoso devuelve 201")]
    public async Task Create_Exitoso_Devuelve201()
    {
        var response = new RolResponse { Id = 1, Nombre = "Nuevo" };
        _service.CreateAsync(Arg.Any<CreateRolRequest>())
            .Returns(Result<RolResponse>.Success(response));

        var result = await _sut.Create(new CreateRolRequest { Nombre = "Nuevo" });

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, objectResult.StatusCode);
    }

    [Fact(DisplayName = "Update: exitoso devuelve 204")]
    public async Task Update_Exitoso_Devuelve204()
    {
        _service.UpdateAsync(1, Arg.Any<UpdateRolRequest>()).Returns(Result.Success());

        var result = await _sut.Update(1, new UpdateRolRequest { Nombre = "Editado" });

        Assert.IsType<NoContentResult>(result);
    }

    [Fact(DisplayName = "Delete: exitoso devuelve 204")]
    public async Task Delete_Exitoso_Devuelve204()
    {
        _service.DeleteAsync(1).Returns(Result.Success());

        var result = await _sut.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }
}
