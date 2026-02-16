using ERPNet.Application.Auth;
using ERPNet.Application.Auth.Interfaces;
using ERPNet.Application.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Enums;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.UnitTests.Services;

public class MaquinariaServiceTests
{
    private readonly IMaquinariaRepository _repo = Substitute.For<IMaquinariaRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserProvider _currentUser = Substitute.For<ICurrentUserProvider>();
    private readonly MaquinariaService _sut;

    public MaquinariaServiceTests()
    {
        _sut = new MaquinariaService(_repo, _uow, _currentUser);
    }

    private void SetupUsuario(int seccionId = 1, Alcance alcance = Alcance.Global)
    {
        var permisos = new List<PermisoUsuario>
        {
            new(RecursoCodigo.Maquinaria, true, true, true, alcance)
        };
        _currentUser.Current.Returns(new UsuarioContext(1, "test@test.com", 1, seccionId, permisos, [1], false));
    }

    private static Maquinaria CrearMaquinaria(int id = 1, int? seccionId = 1) => new()
    {
        Id = id,
        Nombre = "Torno CNC",
        Codigo = $"MAQ-{id:D3}",
        Activa = true,
        SeccionId = seccionId
    };

    #region GetAllAsync

    [Fact(DisplayName = "GetAll: Alcance Global devuelve todas")]
    public async Task GetAll_Global_DevuelveTodas()
    {
        SetupUsuario(alcance: Alcance.Global);
        _repo.GetPaginatedAsync(Arg.Any<PaginacionFilter>(), Alcance.Global, 1)
            .Returns((new List<Maquinaria> { CrearMaquinaria(1), CrearMaquinaria(2) }, 2));

        var result = await _sut.GetAllAsync(new PaginacionFilter());

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalRegistros);
    }

    [Fact(DisplayName = "GetAll: Alcance Seccion filtra por seccion")]
    public async Task GetAll_Seccion_FiltraPorSeccion()
    {
        SetupUsuario(seccionId: 3, alcance: Alcance.Seccion);
        _repo.GetPaginatedAsync(Arg.Any<PaginacionFilter>(), Alcance.Seccion, 3)
            .Returns((new List<Maquinaria> { CrearMaquinaria(1, seccionId: 3) }, 1));

        var result = await _sut.GetAllAsync(new PaginacionFilter());

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.TotalRegistros);
    }

    [Fact(DisplayName = "GetAll: Alcance Propio devuelve lista vacia")]
    public async Task GetAll_Propio_DevuelveVacio()
    {
        SetupUsuario(alcance: Alcance.Propio);
        _repo.GetPaginatedAsync(Arg.Any<PaginacionFilter>(), Alcance.Propio, 1)
            .Returns((new List<Maquinaria>(), 0));

        var result = await _sut.GetAllAsync(new PaginacionFilter());

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetById: con acceso Global devuelve OK")]
    public async Task GetById_Global_DevuelveOk()
    {
        SetupUsuario(alcance: Alcance.Global);
        _repo.GetByIdAsync(1).Returns(CrearMaquinaria());

        var result = await _sut.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal("Torno CNC", result.Value!.Nombre);
    }

    [Fact(DisplayName = "GetById: inexistente devuelve NotFound")]
    public async Task GetById_Inexistente_DevuelveNotFound()
    {
        SetupUsuario();
        _repo.GetByIdAsync(99).Returns((Maquinaria?)null);

        var result = await _sut.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact(DisplayName = "GetById: Alcance Seccion otra seccion devuelve Forbidden")]
    public async Task GetById_SeccionDistinta_DevuelveForbidden()
    {
        SetupUsuario(seccionId: 1, alcance: Alcance.Seccion);
        _repo.GetByIdAsync(1).Returns(CrearMaquinaria(1, seccionId: 99));

        var result = await _sut.GetByIdAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
    }

    [Fact(DisplayName = "GetById: Alcance Propio siempre devuelve Forbidden")]
    public async Task GetById_Propio_DevuelveForbidden()
    {
        SetupUsuario(alcance: Alcance.Propio);
        _repo.GetByIdAsync(1).Returns(CrearMaquinaria());

        var result = await _sut.GetByIdAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "Create: codigo unico crea maquinaria")]
    public async Task Create_CodigoUnico_Crea()
    {
        SetupUsuario();
        _repo.ExisteCodigoAsync("MAQ-001").Returns(false);

        var result = await _sut.CreateAsync(new CreateMaquinariaRequest
        {
            Nombre = "Fresadora",
            Codigo = "MAQ-001"
        });

        Assert.True(result.IsSuccess);
        await _repo.Received(1).CreateAsync(Arg.Any<Maquinaria>());
    }

    [Fact(DisplayName = "Create: codigo duplicado devuelve Conflict")]
    public async Task Create_CodigoDuplicado_DevuelveConflict()
    {
        SetupUsuario();
        _repo.ExisteCodigoAsync("MAQ-001").Returns(true);

        var result = await _sut.CreateAsync(new CreateMaquinariaRequest
        {
            Nombre = "Fresadora",
            Codigo = "MAQ-001"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = "Update: codigo duplicado devuelve Conflict")]
    public async Task Update_CodigoDuplicado_DevuelveConflict()
    {
        SetupUsuario(alcance: Alcance.Global);
        _repo.GetByIdAsync(1).Returns(CrearMaquinaria());
        _repo.ExisteCodigoAsync("OTRO", 1).Returns(true);

        var result = await _sut.UpdateAsync(1, new UpdateMaquinariaRequest { Codigo = "OTRO" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
    }

    [Fact(DisplayName = "Update: sin acceso devuelve Forbidden")]
    public async Task Update_SinAcceso_DevuelveForbidden()
    {
        SetupUsuario(seccionId: 1, alcance: Alcance.Seccion);
        _repo.GetByIdAsync(1).Returns(CrearMaquinaria(1, seccionId: 99));

        var result = await _sut.UpdateAsync(1, new UpdateMaquinariaRequest { Nombre = "X" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = "Delete: con acceso elimina")]
    public async Task Delete_ConAcceso_Elimina()
    {
        SetupUsuario(alcance: Alcance.Global);
        var maq = CrearMaquinaria();
        _repo.GetByIdAsync(1).Returns(maq);

        var result = await _sut.DeleteAsync(1);

        Assert.True(result.IsSuccess);
        _repo.Received(1).Delete(maq);
    }

    [Fact(DisplayName = "Delete: sin acceso devuelve Forbidden")]
    public async Task Delete_SinAcceso_DevuelveForbidden()
    {
        SetupUsuario(alcance: Alcance.Propio);
        _repo.GetByIdAsync(1).Returns(CrearMaquinaria());

        var result = await _sut.DeleteAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
    }

    #endregion
}
