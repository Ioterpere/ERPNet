using ERPNet.Application.Auth.DTOs;
using ERPNet.Application.Auth.Interfaces;
using ERPNet.Application.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Enums;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.UnitTests.Services;

public class MenuServiceTests
{
    private readonly IMenuRepository _repo = Substitute.For<IMenuRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserProvider _currentUser = Substitute.For<ICurrentUserProvider>();
    private readonly MenuService _sut;

    public MenuServiceTests()
    {
        _sut = new MenuService(_repo, _uow, _currentUser);
    }

    private static Menu CrearMenu(int id = 1) => new()
    {
        Id = id,
        Nombre = $"Menu{id}",
        Orden = id,
        Plataforma = Plataforma.WebBlazor,
        SubMenus = []
    };

    #region GetMenusVisiblesAsync

    [Fact(DisplayName = "GetMenusVisibles: devuelve menus mapeados")]
    public async Task GetMenusVisibles_DevuelveMenusMapeados()
    {
        var menus = new List<Menu> { CrearMenu(1), CrearMenu(2) };
        _currentUser.Current.Returns(new UsuarioContext
        {
            Email = "test@test.com", Plataforma = Plataforma.WebBlazor
        });
        _repo.GetMenusVisiblesAsync(Plataforma.WebBlazor, Arg.Any<List<int>>()).Returns(menus);

        var result = await _sut.GetMenusVisiblesAsync([1, 2]);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Equal("Menu1", result.Value[0].Nombre);
    }

    [Fact(DisplayName = "GetMenusVisibles: sin plataforma devuelve lista vacía")]
    public async Task GetMenusVisibles_SinPlataforma_DevuelveVacio()
    {
        _currentUser.Current.Returns(new UsuarioContext { Email = "test@test.com" });

        var result = await _sut.GetMenusVisiblesAsync([1, 2]);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetById: menu existente devuelve OK")]
    public async Task GetById_Existente_DevuelveOk()
    {
        _repo.GetByIdAsync(1).Returns(CrearMenu());

        var result = await _sut.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal("Menu1", result.Value!.Nombre);
    }

    [Fact(DisplayName = "GetById: menu inexistente devuelve NotFound")]
    public async Task GetById_Inexistente_DevuelveNotFound()
    {
        _repo.GetByIdAsync(99).Returns((Menu?)null);

        var result = await _sut.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "Create: crea menu correctamente")]
    public async Task Create_CreaMenu()
    {
        var request = new CreateMenuRequest
        {
            Nombre = "Dashboard",
            Orden = 1,
            Plataforma = Plataforma.WebBlazor
        };

        var result = await _sut.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("Dashboard", result.Value!.Nombre);
        await _repo.Received(1).CreateAsync(Arg.Any<Menu>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region GetRolesAsync

    [Fact(DisplayName = "GetRoles: menu existente devuelve rolIds")]
    public async Task GetRoles_Existente_DevuelveRolIds()
    {
        _repo.GetByIdAsync(1).Returns(CrearMenu());
        _repo.GetRolIdsAsync(1).Returns([1, 2, 3]);

        var result = await _sut.GetRolesAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Count);
    }

    [Fact(DisplayName = "GetRoles: menu inexistente devuelve NotFound")]
    public async Task GetRoles_Inexistente_DevuelveNotFound()
    {
        _repo.GetByIdAsync(99).Returns((Menu?)null);

        var result = await _sut.GetRolesAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion

    #region AsignarRolesAsync

    [Fact(DisplayName = "AsignarRoles: menu existente sincroniza roles")]
    public async Task AsignarRoles_Existente_SincronizaRoles()
    {
        _repo.GetByIdAsync(1).Returns(CrearMenu());

        var result = await _sut.AsignarRolesAsync(1, new AsignarRolesRequest { RolIds = [2, 3] });

        Assert.True(result.IsSuccess);
        await _repo.Received(1).SincronizarRolesAsync(1, Arg.Is<List<int>>(l => l.Contains(2) && l.Contains(3)));
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "AsignarRoles: menu inexistente devuelve NotFound")]
    public async Task AsignarRoles_Inexistente_DevuelveNotFound()
    {
        _repo.GetByIdAsync(99).Returns((Menu?)null);

        var result = await _sut.AsignarRolesAsync(99, new AsignarRolesRequest { RolIds = [1] });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion

}
