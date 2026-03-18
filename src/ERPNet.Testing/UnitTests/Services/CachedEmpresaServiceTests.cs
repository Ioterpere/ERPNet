using ERPNet.Application.Cache;
using ERPNet.Application.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Enums;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Infrastructure.Cache;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.UnitTests.Services;

public class CachedEmpresaServiceTests
{
    private readonly IEmpresaService _inner = Substitute.For<IEmpresaService>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly CachedEmpresaService _sut;

    public CachedEmpresaServiceTests()
    {
        _sut = new CachedEmpresaService(_inner, _cache);
    }

    #region SincronizarEmpresas invalida caché

    [Fact(DisplayName = "SincronizarEmpresas: éxito invalida caché del usuario")]
    public async Task SincronizarEmpresas_Exito_InvalidaCacheUsuario()
    {
        _inner.SincronizarEmpresasDeUsuarioAsync(5, Arg.Any<AsignarEmpresasRequest>())
            .Returns(Result.Success());

        await _sut.SincronizarEmpresasDeUsuarioAsync(5, new AsignarEmpresasRequest());

        _cache.Received(1).RemoveByPrefix("usuario:5:");
        _cache.Received(1).RemoveByPrefix("menu:5:");
    }

    [Fact(DisplayName = "SincronizarEmpresas: fallo no invalida caché")]
    public async Task SincronizarEmpresas_Fallo_NoInvalidaCache()
    {
        _inner.SincronizarEmpresasDeUsuarioAsync(5, Arg.Any<AsignarEmpresasRequest>())
            .Returns(Result.Failure("error", ErrorType.NotFound));

        await _sut.SincronizarEmpresasDeUsuarioAsync(5, new AsignarEmpresasRequest());

        _cache.DidNotReceive().RemoveByPrefix(Arg.Any<string>());
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

    [Fact(DisplayName = "GetById: delega sin tocar caché")]
    public async Task GetById_DelegaSinCache()
    {
        await _sut.GetByIdAsync(1);

        await _inner.Received(1).GetByIdAsync(1);
        _cache.DidNotReceive().RemoveByPrefix(Arg.Any<string>());
    }

    #endregion
}
