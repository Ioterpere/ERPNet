using ERPNet.Application.Auth.DTOs;
using ERPNet.Application.Auth.Interfaces;
using ERPNet.Application.Cache;
using ERPNet.Application.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Enums;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Enums;
using ERPNet.Infrastructure.Cache;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.UnitTests.Services;

public class CachedMenuServiceTests
{
    private readonly IMenuService _inner = Substitute.For<IMenuService>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly ICurrentUserProvider _currentUser = Substitute.For<ICurrentUserProvider>();
    private readonly CachedMenuService _sut;

    public CachedMenuServiceTests()
    {
        _sut = new CachedMenuService(_inner, _cache, _currentUser);
    }

    private static List<MenuResponse> CrearMenus() =>
    [
        new() { Id = 1, Nombre = "Dashboard", Orden = 1 },
        new() { Id = 2, Nombre = "Admin", Orden = 2 }
    ];

    #region GetMenusVisibles — caché

    [Fact(DisplayName = "GetMenusVisibles: hit de caché no llama al inner")]
    public async Task GetMenusVisibles_CacheHit_NoLlamaInner()
    {
        var ctx = new UsuarioContext { Id = 1, EmpresaId = 10, Email = "u@test.com" };
        _currentUser.Current.Returns(ctx);
        _cache.Get<List<MenuResponse>>("menu:1:10").Returns(CrearMenus());

        var result = await _sut.GetMenusVisiblesAsync([1, 2]);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        await _inner.DidNotReceive().GetMenusVisiblesAsync(Arg.Any<List<int>>());
    }

    [Fact(DisplayName = "GetMenusVisibles: miss de caché llama al inner y almacena resultado")]
    public async Task GetMenusVisibles_CacheMiss_LlamaInnerYAlmacena()
    {
        var ctx = new UsuarioContext { Id = 1, EmpresaId = 10, Email = "u@test.com" };
        _currentUser.Current.Returns(ctx);
        _cache.Get<List<MenuResponse>>("menu:1:10").Returns((List<MenuResponse>?)null);
        _inner.GetMenusVisiblesAsync(Arg.Any<List<int>>())
            .Returns(Result<List<MenuResponse>>.Success(CrearMenus()));

        var result = await _sut.GetMenusVisiblesAsync([1, 2]);

        Assert.True(result.IsSuccess);
        await _inner.Received(1).GetMenusVisiblesAsync(Arg.Any<List<int>>());
        _cache.Received(1).Set("menu:1:10", Arg.Any<List<MenuResponse>>(), Arg.Any<TimeSpan?>());
    }

    [Fact(DisplayName = "GetMenusVisibles: sin usuario activo delega directamente")]
    public async Task GetMenusVisibles_SinUsuario_DelegaDirectamente()
    {
        _currentUser.Current.Returns((UsuarioContext?)null);
        _inner.GetMenusVisiblesAsync(Arg.Any<List<int>>())
            .Returns(Result<List<MenuResponse>>.Success(CrearMenus()));

        var result = await _sut.GetMenusVisiblesAsync([1]);

        Assert.True(result.IsSuccess);
        await _inner.Received(1).GetMenusVisiblesAsync(Arg.Any<List<int>>());
        _cache.DidNotReceive().Get<List<MenuResponse>>(Arg.Any<string>());
    }

    [Fact(DisplayName = "GetMenusVisibles: EmpresaId null usa clave con 0")]
    public async Task GetMenusVisibles_SinEmpresaId_UsaClaveConCero()
    {
        var ctx = new UsuarioContext { Id = 3, EmpresaId = null, Email = "u@test.com" };
        _currentUser.Current.Returns(ctx);
        _cache.Get<List<MenuResponse>>("menu:3:0").Returns((List<MenuResponse>?)null);
        _inner.GetMenusVisiblesAsync(Arg.Any<List<int>>())
            .Returns(Result<List<MenuResponse>>.Success([]));

        await _sut.GetMenusVisiblesAsync([1]);

        _cache.Received(1).Get<List<MenuResponse>>("menu:3:0");
    }

    #endregion

    #region Mutaciones invalidan caché

    [Fact(DisplayName = "Create: éxito invalida caché de menús")]
    public async Task Create_Exito_InvalidaCache()
    {
        _inner.CreateAsync(Arg.Any<CreateMenuRequest>())
            .Returns(Result<MenuResponse>.Success(new MenuResponse { Id = 1, Nombre = "X", Orden = 1 }));

        await _sut.CreateAsync(new CreateMenuRequest { Nombre = "X", Plataforma = Plataforma.WebBlazor });

        _cache.Received(1).RemoveByPrefix("menu:");
    }

    [Fact(DisplayName = "Create: fallo no invalida caché")]
    public async Task Create_Fallo_NoInvalidaCache()
    {
        _inner.CreateAsync(Arg.Any<CreateMenuRequest>())
            .Returns(Result<MenuResponse>.Failure("error", ErrorType.Validation));

        await _sut.CreateAsync(new CreateMenuRequest { Nombre = "X", Plataforma = Plataforma.WebBlazor });

        _cache.DidNotReceive().RemoveByPrefix(Arg.Any<string>());
    }

    [Fact(DisplayName = "Update: éxito invalida caché de menús")]
    public async Task Update_Exito_InvalidaCache()
    {
        _inner.UpdateAsync(1, Arg.Any<UpdateMenuRequest>())
            .Returns(Result<MenuResponse>.Success(new MenuResponse { Id = 1, Nombre = "Y", Orden = 1 }));

        await _sut.UpdateAsync(1, new UpdateMenuRequest { Nombre = "Y" });

        _cache.Received(1).RemoveByPrefix("menu:");
    }

    [Fact(DisplayName = "Delete: éxito invalida caché de menús")]
    public async Task Delete_Exito_InvalidaCache()
    {
        _inner.DeleteAsync(1).Returns(Result.Success());

        await _sut.DeleteAsync(1);

        _cache.Received(1).RemoveByPrefix("menu:");
    }

    [Fact(DisplayName = "AsignarRoles: éxito invalida caché de menús")]
    public async Task AsignarRoles_Exito_InvalidaCache()
    {
        _inner.AsignarRolesAsync(1, Arg.Any<AsignarRolesRequest>()).Returns(Result.Success());

        await _sut.AsignarRolesAsync(1, new AsignarRolesRequest());

        _cache.Received(1).RemoveByPrefix("menu:");
    }

    #endregion
}
