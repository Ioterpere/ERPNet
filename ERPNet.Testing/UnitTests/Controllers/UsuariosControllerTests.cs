using ERPNet.Api.Controllers;
using ERPNet.Application.Auth;
using ERPNet.Application.Auth.DTOs;
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

public class UsuariosControllerTests
{
    private readonly IUsuarioService _service = Substitute.For<IUsuarioService>();
    private readonly UsuariosController _sut;

    public UsuariosControllerTests()
    {
        _sut = new UsuariosController(_service);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    private void SetupUsuarioContext(int id = 1, List<int>? rolIds = null)
    {
        _sut.HttpContext.Items["UsuarioContext"] = new UsuarioContext(
            id, "test@test.com", 1, 1, [], rolIds ?? [1], false);
    }

    #region CRUD

    [Fact(DisplayName = "GetAll: exitoso devuelve 200")]
    public async Task GetAll_Exitoso_Devuelve200()
    {
        var lista = new ListaPaginada<UsuarioResponse>
        {
            Items = [new UsuarioResponse { Id = 1, Email = "a@a.com" }],
            Pagina = 1, PorPagina = 50, TotalRegistros = 1
        };
        _service.GetAllAsync(Arg.Any<PaginacionFilter>())
            .Returns(Result<ListaPaginada<UsuarioResponse>>.Success(lista));

        var result = await _sut.GetAll(new PaginacionFilter());

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsType<ListaPaginada<UsuarioResponse>>(okResult.Value);
        Assert.Single(value.Items);
    }

    [Fact(DisplayName = "GetById: existente devuelve 200")]
    public async Task GetById_Existente_Devuelve200()
    {
        var response = new UsuarioResponse { Id = 1, Email = "a@a.com" };
        _service.GetByIdAsync(1).Returns(Result<UsuarioResponse>.Success(response));

        var result = await _sut.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, ((UsuarioResponse)okResult.Value!).Id);
    }

    [Fact(DisplayName = "GetById: inexistente devuelve 404")]
    public async Task GetById_Inexistente_Devuelve404()
    {
        _service.GetByIdAsync(99).Returns(Result<UsuarioResponse>.Failure("No encontrado", ErrorType.NotFound));

        var result = await _sut.GetById(99);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact(DisplayName = "Create: exitoso devuelve 201")]
    public async Task Create_Exitoso_Devuelve201()
    {
        var response = new UsuarioResponse { Id = 1, Email = "new@test.com" };
        _service.CreateAsync(Arg.Any<CreateUsuarioRequest>())
            .Returns(Result<UsuarioResponse>.Success(response));

        var result = await _sut.Create(new CreateUsuarioRequest
        {
            Email = "new@test.com", Password = "Pass123!", EmpleadoId = 1
        });

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, objectResult.StatusCode);
    }

    [Fact(DisplayName = "Create: email duplicado devuelve 409")]
    public async Task Create_EmailDuplicado_Devuelve409()
    {
        _service.CreateAsync(Arg.Any<CreateUsuarioRequest>())
            .Returns(Result<UsuarioResponse>.Failure("Email ya existe", ErrorType.Conflict));

        var result = await _sut.Create(new CreateUsuarioRequest
        {
            Email = "dup@test.com", Password = "Pass123!", EmpleadoId = 1
        });

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(409, objectResult.StatusCode);
    }

    [Fact(DisplayName = "Update: exitoso devuelve 204")]
    public async Task Update_Exitoso_Devuelve204()
    {
        _service.UpdateAsync(1, Arg.Any<UpdateUsuarioRequest>()).Returns(Result.Success());

        var result = await _sut.Update(1, new UpdateUsuarioRequest { Email = "upd@test.com" });

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

    #region Account / Contrasena

    [Fact(DisplayName = "GetMe: devuelve AccountResponse con datos del usuario")]
    public async Task GetMe_DevuelveAccountResponse()
    {
        SetupUsuarioContext(id: 5, rolIds: [1, 2]);

        var result = _sut.GetMe();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var account = Assert.IsType<AccountResponse>(okResult.Value);
        Assert.Equal(5, account.Id);
        Assert.Equal("test@test.com", account.Email);
        Assert.Equal(2, account.Roles.Count);
    }

    [Fact(DisplayName = "CambiarContrasena: exitoso devuelve 204 con UsuarioActual.Id")]
    public async Task CambiarContrasena_Exitoso_Devuelve204()
    {
        SetupUsuarioContext(id: 7);
        _service.CambiarContrasenaAsync(7, Arg.Any<CambiarContrasenaRequest>()).Returns(Result.Success());

        var result = await _sut.CambiarContrasena(new CambiarContrasenaRequest
        {
            ContrasenaActual = "old", NuevaContrasena = "new", ConfirmarContrasena = "new"
        });

        Assert.IsType<NoContentResult>(result);
        await _service.Received(1).CambiarContrasenaAsync(7, Arg.Any<CambiarContrasenaRequest>());
    }

    [Fact(DisplayName = "ResetearContrasena: exitoso devuelve 204")]
    public async Task ResetearContrasena_Exitoso_Devuelve204()
    {
        _service.ResetearContrasenaAsync(1, Arg.Any<ResetearContrasenaRequest>()).Returns(Result.Success());

        var result = await _sut.ResetearContrasena(1, new ResetearContrasenaRequest
        {
            NuevaContrasena = "new", ConfirmarContrasena = "new"
        });

        Assert.IsType<NoContentResult>(result);
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

        var result = await _sut.AsignarRoles(1, new AsignarRolesRequest { RolIds = [1, 2] });

        Assert.IsType<NoContentResult>(result);
    }

    #endregion
}
