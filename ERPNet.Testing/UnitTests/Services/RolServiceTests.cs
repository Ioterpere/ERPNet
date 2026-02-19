using ERPNet.Application.Common;
using ERPNet.Contracts.DTOs;
using ERPNet.Contracts;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.UnitTests.Services;

public class RolServiceTests
{
    private readonly IRolRepository _repo = Substitute.For<IRolRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly RolService _sut;

    public RolServiceTests()
    {
        _sut = new RolService(_repo, _uow, _cache);
    }

    private static Rol CrearRol(int id = 1) => new()
    {
        Id = id,
        Nombre = $"Rol{id}",
        Descripcion = "Test"
    };

    #region GetAllAsync

    [Fact(DisplayName = "GetAll: devuelve lista paginada")]
    public async Task GetAll_DevuelveListaPaginada()
    {
        var roles = new List<Rol> { CrearRol(1), CrearRol(2) };
        _repo.GetPaginatedAsync(Arg.Any<PaginacionFilter>()).Returns((roles, 2));

        var result = await _sut.GetAllAsync(new PaginacionFilter());

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalRegistros);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetById: rol existente devuelve OK")]
    public async Task GetById_Existente_DevuelveOk()
    {
        _repo.GetByIdAsync(1).Returns(CrearRol());

        var result = await _sut.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal("Rol1", result.Value!.Nombre);
    }

    [Fact(DisplayName = "GetById: rol inexistente devuelve NotFound")]
    public async Task GetById_Inexistente_DevuelveNotFound()
    {
        _repo.GetByIdAsync(99).Returns((Rol?)null);

        var result = await _sut.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "Create: nombre unico crea rol")]
    public async Task Create_NombreUnico_CreaRol()
    {
        _repo.ExisteNombreAsync("Admin").Returns(false);

        var result = await _sut.CreateAsync(new CreateRolRequest { Nombre = "Admin" });

        Assert.True(result.IsSuccess);
        await _repo.Received(1).CreateAsync(Arg.Any<Rol>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Create: nombre duplicado devuelve Conflict")]
    public async Task Create_NombreDuplicado_DevuelveConflict()
    {
        _repo.ExisteNombreAsync("Admin").Returns(true);

        var result = await _sut.CreateAsync(new CreateRolRequest { Nombre = "Admin" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = "Update: datos validos actualiza rol")]
    public async Task Update_DatosValidos_ActualizaRol()
    {
        var rol = CrearRol();
        _repo.GetByIdAsync(1).Returns(rol);
        _repo.GetUsuarioIdsPorRol(1).Returns([10, 20]);

        var result = await _sut.UpdateAsync(1, new UpdateRolRequest { Nombre = "SuperAdmin" });

        Assert.True(result.IsSuccess);
        Assert.Equal("SuperAdmin", rol.Nombre);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Update: nombre duplicado devuelve Conflict")]
    public async Task Update_NombreDuplicado_DevuelveConflict()
    {
        _repo.GetByIdAsync(1).Returns(CrearRol());
        _repo.ExisteNombreAsync("Existente", 1).Returns(true);

        var result = await _sut.UpdateAsync(1, new UpdateRolRequest { Nombre = "Existente" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
    }

    [Fact(DisplayName = "Update: inexistente devuelve NotFound")]
    public async Task Update_Inexistente_DevuelveNotFound()
    {
        _repo.GetByIdAsync(99).Returns((Rol?)null);

        var result = await _sut.UpdateAsync(99, new UpdateRolRequest { Nombre = "X" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = "Delete: rol existente lo elimina")]
    public async Task Delete_Existente_Elimina()
    {
        var rol = CrearRol();
        _repo.GetByIdAsync(1).Returns(rol);
        _repo.GetUsuarioIdsPorRol(1).Returns([10]);

        var result = await _sut.DeleteAsync(1);

        Assert.True(result.IsSuccess);
        _repo.Received(1).Delete(rol);
    }

    [Fact(DisplayName = "Delete: inexistente devuelve NotFound")]
    public async Task Delete_Inexistente_DevuelveNotFound()
    {
        _repo.GetByIdAsync(99).Returns((Rol?)null);

        var result = await _sut.DeleteAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion

    #region Invalidación de caché

    [Fact(DisplayName = "Update: invalida cache de todos los usuarios del rol")]
    public async Task Update_InvalidaCacheUsuarios()
    {
        _repo.GetByIdAsync(1).Returns(CrearRol());
        _repo.GetUsuarioIdsPorRol(1).Returns([10, 20, 30]);

        await _sut.UpdateAsync(1, new UpdateRolRequest { Descripcion = "Nueva desc" });

        _cache.Received(1).Remove("usuario:10");
        _cache.Received(1).Remove("usuario:20");
        _cache.Received(1).Remove("usuario:30");
    }

    [Fact(DisplayName = "Delete: invalida cache de todos los usuarios del rol")]
    public async Task Delete_InvalidaCacheUsuarios()
    {
        _repo.GetByIdAsync(1).Returns(CrearRol());
        _repo.GetUsuarioIdsPorRol(1).Returns([5, 15]);

        await _sut.DeleteAsync(1);

        _cache.Received(1).Remove("usuario:5");
        _cache.Received(1).Remove("usuario:15");
    }

    [Fact(DisplayName = "Create: no invalida cache (no hay usuarios asignados)")]
    public async Task Create_NoInvalidaCache()
    {
        _repo.ExisteNombreAsync("Nuevo").Returns(false);

        await _sut.CreateAsync(new CreateRolRequest { Nombre = "Nuevo" });

        _cache.DidNotReceive().Remove(Arg.Any<string>());
    }

    #endregion
}
