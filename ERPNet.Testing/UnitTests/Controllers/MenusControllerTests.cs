using ERPNet.Api.Controllers;
using ERPNet.Application.Auth;
using ERPNet.Application.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Enums;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.UnitTests.Controllers;

public class MenusControllerTests
{
    private readonly IMenuService _service = Substitute.For<IMenuService>();
    private readonly MenusController _sut;

    public MenusControllerTests()
    {
        _sut = new MenusController(_service);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    private void SetupUsuarioContext(List<int>? rolIds = null)
    {
        _sut.HttpContext.Items["UsuarioContext"] = new UsuarioContext(
            1, "test@test.com", 1, 1, [], rolIds ?? [1, 2], false);
    }

    #region GetMenus

    [Fact(DisplayName = "GetMenus: pasa RolIds y Plataforma al service")]
    public async Task GetMenus_PasaRolIdsYPlataforma()
    {
        SetupUsuarioContext(rolIds: [3, 5]);
        _service.GetMenusVisiblesAsync(Arg.Any<Plataforma>(), Arg.Any<List<int>>())
            .Returns(Result<List<MenuResponse>>.Success([]));

        await _sut.GetMenus(Plataforma.Web);

        await _service.Received(1).GetMenusVisiblesAsync(Plataforma.Web, Arg.Is<List<int>>(r => r.Contains(3) && r.Contains(5)));
    }

    [Fact(DisplayName = "GetMenus: exitoso devuelve 200")]
    public async Task GetMenus_Exitoso_Devuelve200()
    {
        SetupUsuarioContext();
        var menus = new List<MenuResponse>
        {
            new() { Id = 1, Nombre = "Dashboard", Orden = 1 }
        };
        _service.GetMenusVisiblesAsync(Arg.Any<Plataforma>(), Arg.Any<List<int>>())
            .Returns(Result<List<MenuResponse>>.Success(menus));

        var result = await _sut.GetMenus(Plataforma.Web);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<List<MenuResponse>>(okResult.Value);
        Assert.Single(value);
    }

    #endregion

    #region GetById

    [Fact(DisplayName = "GetById: existente devuelve 200")]
    public async Task GetById_Existente_Devuelve200()
    {
        var response = new MenuResponse { Id = 1, Nombre = "Dashboard", Orden = 1 };
        _service.GetByIdAsync(1).Returns(Result<MenuResponse>.Success(response));

        var result = await _sut.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Dashboard", ((MenuResponse)okResult.Value!).Nombre);
    }

    [Fact(DisplayName = "GetById: inexistente devuelve 404")]
    public async Task GetById_Inexistente_Devuelve404()
    {
        _service.GetByIdAsync(99).Returns(Result<MenuResponse>.Failure("No encontrado", ErrorType.NotFound));

        var result = await _sut.GetById(99);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    #endregion

    #region Create

    [Fact(DisplayName = "Create: exitoso devuelve 201")]
    public async Task Create_Exitoso_Devuelve201()
    {
        var response = new MenuResponse { Id = 1, Nombre = "Nuevo", Orden = 1 };
        _service.CreateAsync(Arg.Any<CreateMenuRequest>())
            .Returns(Result<MenuResponse>.Success(response));

        var result = await _sut.Create(new CreateMenuRequest
        {
            Nombre = "Nuevo", Orden = 1, Plataforma = Plataforma.Web, RolIds = [1]
        });

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, objectResult.StatusCode);
    }

    #endregion

    #region Roles

    [Fact(DisplayName = "GetRoles: exitoso devuelve 200")]
    public async Task GetRoles_Exitoso_Devuelve200()
    {
        _service.GetRolesAsync(1).Returns(Result<List<int>>.Success([1, 2]));

        var result = await _sut.GetRoles(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var roles = Assert.IsType<List<int>>(okResult.Value);
        Assert.Equal(2, roles.Count);
    }

    [Fact(DisplayName = "AsignarRoles: exitoso devuelve 204")]
    public async Task AsignarRoles_Exitoso_Devuelve204()
    {
        _service.AsignarRolesAsync(1, Arg.Any<AsignarRolesRequest>()).Returns(Result.Success());

        var result = await _sut.AsignarRoles(1, new AsignarRolesRequest { RolIds = [1] });

        Assert.IsType<NoContentResult>(result);
    }

    #endregion
}
