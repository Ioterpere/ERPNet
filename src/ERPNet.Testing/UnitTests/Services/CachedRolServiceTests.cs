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

public class CachedRolServiceTests
{
    private readonly IRolService _inner = Substitute.For<IRolService>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly CachedRolService _sut;

    public CachedRolServiceTests()
    {
        _sut = new CachedRolService(_inner, _cache);
    }

    #region Update invalida caché

    [Fact(DisplayName = "Update: éxito invalida caché global de usuarios y menús")]
    public async Task Update_Exito_InvalidaCacheGlobal()
    {
        _inner.UpdateAsync(1, Arg.Any<UpdateRolRequest>()).Returns(Result.Success());

        await _sut.UpdateAsync(1, new UpdateRolRequest());

        _cache.Received(1).RemoveByPrefix("usuario:");
        _cache.Received(1).RemoveByPrefix("menu:");
    }

    [Fact(DisplayName = "Update: fallo no invalida caché")]
    public async Task Update_Fallo_NoInvalidaCache()
    {
        _inner.UpdateAsync(1, Arg.Any<UpdateRolRequest>()).Returns(Result.Failure("error", ErrorType.NotFound));

        await _sut.UpdateAsync(1, new UpdateRolRequest());

        _cache.DidNotReceive().RemoveByPrefix(Arg.Any<string>());
    }

    #endregion

    #region Delete invalida caché

    [Fact(DisplayName = "Delete: éxito invalida caché global de usuarios y menús")]
    public async Task Delete_Exito_InvalidaCacheGlobal()
    {
        _inner.DeleteAsync(1).Returns(Result.Success());

        await _sut.DeleteAsync(1);

        _cache.Received(1).RemoveByPrefix("usuario:");
        _cache.Received(1).RemoveByPrefix("menu:");
    }

    [Fact(DisplayName = "Delete: fallo no invalida caché")]
    public async Task Delete_Fallo_NoInvalidaCache()
    {
        _inner.DeleteAsync(1).Returns(Result.Failure("error", ErrorType.NotFound));

        await _sut.DeleteAsync(1);

        _cache.DidNotReceive().RemoveByPrefix(Arg.Any<string>());
    }

    #endregion

    #region SetPermisos invalida caché

    [Fact(DisplayName = "SetPermisos: éxito invalida caché global")]
    public async Task SetPermisos_Exito_InvalidaCacheGlobal()
    {
        _inner.SetPermisosAsync(1, Arg.Any<SetPermisosRolRequest>()).Returns(Result.Success());

        await _sut.SetPermisosAsync(1, new SetPermisosRolRequest());

        _cache.Received(1).RemoveByPrefix("usuario:");
        _cache.Received(1).RemoveByPrefix("menu:");
    }

    #endregion

    #region SincronizarTodasAsignacionesUsuario invalida caché

    [Fact(DisplayName = "SincronizarAsignaciones: éxito invalida caché global")]
    public async Task SincronizarAsignaciones_Exito_InvalidaCacheGlobal()
    {
        _inner.SincronizarTodasAsignacionesUsuarioAsync(1, Arg.Any<List<AsignacionUsuarioDto>>())
            .Returns(Result.Success());

        await _sut.SincronizarTodasAsignacionesUsuarioAsync(1, []);

        _cache.Received(1).RemoveByPrefix("usuario:");
        _cache.Received(1).RemoveByPrefix("menu:");
    }

    #endregion

    #region Delegaciones sin caché

    [Fact(DisplayName = "Create: delega sin tocar caché")]
    public async Task Create_DelegaSinCache()
    {
        await _sut.CreateAsync(new CreateRolRequest { Nombre = "Test" });

        await _inner.Received(1).CreateAsync(Arg.Any<CreateRolRequest>());
        _cache.DidNotReceive().RemoveByPrefix(Arg.Any<string>());
    }

    [Fact(DisplayName = "GetAll: delega sin tocar caché")]
    public async Task GetAll_DelegaSinCache()
    {
        await _sut.GetAllAsync(new Domain.Filters.PaginacionFilter());

        await _inner.Received(1).GetAllAsync(Arg.Any<Domain.Filters.PaginacionFilter>());
        _cache.DidNotReceive().RemoveByPrefix(Arg.Any<string>());
    }

    #endregion
}
