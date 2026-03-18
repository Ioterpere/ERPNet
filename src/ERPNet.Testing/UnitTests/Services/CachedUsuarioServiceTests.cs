using ERPNet.Application.Auth.DTOs;
using ERPNet.Application.Cache;
using ERPNet.Application.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Enums;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Infrastructure.Cache;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.UnitTests.Services;

public class CachedUsuarioServiceTests
{
    private readonly IUsuarioService _inner = Substitute.For<IUsuarioService>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly CachedUsuarioService _sut;

    public CachedUsuarioServiceTests()
    {
        _sut = new CachedUsuarioService(_inner, _cache);
    }

    #region Update invalida caché

    [Fact(DisplayName = "Update: éxito invalida caché del usuario")]
    public async Task Update_Exito_InvalidaCache()
    {
        _inner.UpdateAsync(1, Arg.Any<UpdateUsuarioRequest>()).Returns(Result.Success());

        await _sut.UpdateAsync(1, new UpdateUsuarioRequest());

        _cache.Received(1).RemoveByPrefix("usuario:1:");
        _cache.Received(1).RemoveByPrefix("menu:1:");
    }

    [Fact(DisplayName = "Update: fallo no invalida caché")]
    public async Task Update_Fallo_NoInvalidaCache()
    {
        _inner.UpdateAsync(1, Arg.Any<UpdateUsuarioRequest>())
            .Returns(Result.Failure("error", ErrorType.NotFound));

        await _sut.UpdateAsync(1, new UpdateUsuarioRequest());

        _cache.DidNotReceive().RemoveByPrefix(Arg.Any<string>());
    }

    #endregion

    #region Delete invalida caché

    [Fact(DisplayName = "Delete: éxito invalida caché del usuario")]
    public async Task Delete_Exito_InvalidaCache()
    {
        _inner.DeleteAsync(1).Returns(Result.Success());

        await _sut.DeleteAsync(1);

        _cache.Received(1).RemoveByPrefix("usuario:1:");
        _cache.Received(1).RemoveByPrefix("menu:1:");
    }

    [Fact(DisplayName = "Delete: fallo no invalida caché")]
    public async Task Delete_Fallo_NoInvalidaCache()
    {
        _inner.DeleteAsync(1).Returns(Result.Failure("error", ErrorType.NotFound));

        await _sut.DeleteAsync(1);

        _cache.DidNotReceive().RemoveByPrefix(Arg.Any<string>());
    }

    #endregion

    #region CambiarContrasena invalida caché

    [Fact(DisplayName = "CambiarContrasena: éxito invalida caché del usuario")]
    public async Task CambiarContrasena_Exito_InvalidaCache()
    {
        _inner.CambiarContrasenaAsync(1, Arg.Any<CambiarContrasenaRequest>())
            .Returns(Result.Success());

        await _sut.CambiarContrasenaAsync(1, new CambiarContrasenaRequest
        {
            ContrasenaActual = "old", NuevaContrasena = "new1!", ConfirmarContrasena = "new1!"
        });

        _cache.Received(1).RemoveByPrefix("usuario:1:");
        _cache.Received(1).RemoveByPrefix("menu:1:");
    }

    #endregion

    #region ResetearContrasena invalida caché

    [Fact(DisplayName = "ResetearContrasena: éxito invalida caché del usuario")]
    public async Task ResetearContrasena_Exito_InvalidaCache()
    {
        _inner.ResetearContrasenaAsync(1).Returns(Result.Success());

        await _sut.ResetearContrasenaAsync(1);

        _cache.Received(1).RemoveByPrefix("usuario:1:");
        _cache.Received(1).RemoveByPrefix("menu:1:");
    }

    #endregion

    #region AsignarRoles invalida caché

    [Fact(DisplayName = "AsignarRoles: éxito invalida caché del usuario")]
    public async Task AsignarRoles_Exito_InvalidaCache()
    {
        _inner.AsignarRolesAsync(1, Arg.Any<AsignarRolesRequest>(), Arg.Any<int?>())
            .Returns(Result.Success());

        await _sut.AsignarRolesAsync(1, new AsignarRolesRequest());

        _cache.Received(1).RemoveByPrefix("usuario:1:");
        _cache.Received(1).RemoveByPrefix("menu:1:");
    }

    #endregion

    #region Delegaciones sin caché

    [Fact(DisplayName = "GetAll: delega sin tocar caché")]
    public async Task GetAll_DelegaSinCache()
    {
        await _sut.GetAllAsync(new Domain.Filters.PaginacionFilter());

        await _inner.Received(1).GetAllAsync(Arg.Any<Domain.Filters.PaginacionFilter>());
        _cache.DidNotReceive().RemoveByPrefix(Arg.Any<string>());
    }

    #endregion
}
