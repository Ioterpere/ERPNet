using ERPNet.Application.Auth.DTOs;
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

public class CuentaServiceTests
{
    private readonly ICuentaRepository _cuentaRepo = Substitute.For<ICuentaRepository>();
    private readonly IApunteContableRepository _apunteRepo = Substitute.For<IApunteContableRepository>();
    private readonly ITipoDiarioRepository _tipoDiarioRepo = Substitute.For<ITipoDiarioRepository>();
    private readonly ICentroCosteRepository _centroCosteRepo = Substitute.For<ICentroCosteRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserProvider _currentUser = Substitute.For<ICurrentUserProvider>();
    private readonly CuentaService _sut;

    public CuentaServiceTests()
    {
        _sut = new CuentaService(_cuentaRepo, _apunteRepo, _tipoDiarioRepo, _centroCosteRepo, _uow, _currentUser);
    }

    private void SetupUsuario(int empresaId = 1)
    {
        _currentUser.Current.Returns(new UsuarioContext
        {
            Id = 1, Email = "test@test.com", EmpresaId = empresaId,
            Permisos = [new PermisoResponse { Codigo = RecursoCodigo.Contabilidad, CanCreate = true, CanEdit = true, CanDelete = true, Alcance = Alcance.Global }],
            RolIds = [1]
        });
    }

    private static Cuenta CrearCuenta(int id = 1, int empresaId = 1) => new()
    {
        Id = id,
        Codigo = "10000000",
        Descripcion = "Cuenta de prueba",
        EmpresaId = empresaId
    };

    #region GetAllAsync

    [Fact(DisplayName = "GetAll: devuelve lista paginada")]
    public async Task GetAll_DevuelveListaPaginada()
    {
        var cuentas = new List<Cuenta> { CrearCuenta(1), CrearCuenta(2) };
        _cuentaRepo.GetPaginatedAsync(Arg.Any<CuentaFilter>()).Returns((cuentas, 2));

        var result = await _sut.GetAllAsync(new CuentaFilter());

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalRegistros);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetById: existente devuelve OK")]
    public async Task GetById_Existente_DevuelveOk()
    {
        _cuentaRepo.GetByIdAsync(1).Returns(CrearCuenta());

        var result = await _sut.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal("10000000", result.Value!.Codigo);
    }

    [Fact(DisplayName = "GetById: inexistente devuelve NotFound")]
    public async Task GetById_Inexistente_DevuelveNotFound()
    {
        _cuentaRepo.GetByIdAsync(99).Returns((Cuenta?)null);

        var result = await _sut.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "Create: datos válidos crea cuenta")]
    public async Task Create_DatosValidos_CreaCuenta()
    {
        SetupUsuario();
        _cuentaRepo.ExisteCodigoAsync("10000000", 1).Returns(false);

        var result = await _sut.CreateAsync(new CreateCuentaRequest { Codigo = "10000000", Descripcion = "Nueva cuenta" });

        Assert.True(result.IsSuccess);
        Assert.Equal("10000000", result.Value!.Codigo);
        await _cuentaRepo.Received(1).CreateAsync(Arg.Any<Cuenta>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Create: código duplicado devuelve Conflict")]
    public async Task Create_CodigoDuplicado_DevuelveConflict()
    {
        SetupUsuario();
        _cuentaRepo.ExisteCodigoAsync("10000000", 1).Returns(true);

        var result = await _sut.CreateAsync(new CreateCuentaRequest { Codigo = "10000000", Descripcion = "X" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = "Update: datos válidos actualiza cuenta")]
    public async Task Update_DatosValidos_ActualizaCuenta()
    {
        var cuenta = CrearCuenta();
        _cuentaRepo.GetByIdAsync(1).Returns(cuenta);

        var result = await _sut.UpdateAsync(1, new UpdateCuentaRequest { Descripcion = "Descripción nueva" });

        Assert.True(result.IsSuccess);
        Assert.Equal("Descripción nueva", cuenta.Descripcion);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Update: inexistente devuelve NotFound")]
    public async Task Update_Inexistente_DevuelveNotFound()
    {
        _cuentaRepo.GetByIdAsync(99).Returns((Cuenta?)null);

        var result = await _sut.UpdateAsync(99, new UpdateCuentaRequest { Descripcion = "X" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact(DisplayName = "Update: cuenta padre = id devuelve Validation")]
    public async Task Update_CuentaPadreIgualId_DevuelveValidation()
    {
        _cuentaRepo.GetByIdAsync(1).Returns(CrearCuenta());

        var result = await _sut.UpdateAsync(1, new UpdateCuentaRequest { CuentaPadreId = 1 });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = "Delete: existente elimina cuenta")]
    public async Task Delete_Existente_Elimina()
    {
        var cuenta = CrearCuenta();
        _cuentaRepo.GetByIdAsync(1).Returns(cuenta);

        var result = await _sut.DeleteAsync(1);

        Assert.True(result.IsSuccess);
        _cuentaRepo.Received(1).Delete(cuenta);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Delete: inexistente devuelve NotFound")]
    public async Task Delete_Inexistente_DevuelveNotFound()
    {
        _cuentaRepo.GetByIdAsync(99).Returns((Cuenta?)null);

        var result = await _sut.DeleteAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion

    #region GetExtractoAsync

    [Fact(DisplayName = "GetExtracto: cuenta existente devuelve apuntes")]
    public async Task GetExtracto_CuentaExistente_DevuelveApuntes()
    {
        _cuentaRepo.GetByIdAsync(1).Returns(CrearCuenta());
        _apunteRepo.GetExtractoAsync(1, Arg.Any<ExtractoFilter>()).Returns(new List<ApunteContable>
        {
            new() { Id = 1, CuentaId = 1, Asiento = 1, NumLinea = 1, NumDiario = 1, Fecha = DateOnly.FromDateTime(DateTime.Today), Concepto = "Test", Debe = 100, Haber = 0, EmpresaId = 1 }
        });

        var result = await _sut.GetExtractoAsync(1, new ExtractoFilter());

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    [Fact(DisplayName = "GetExtracto: cuenta inexistente devuelve NotFound")]
    public async Task GetExtracto_CuentaInexistente_DevuelveNotFound()
    {
        _cuentaRepo.GetByIdAsync(99).Returns((Cuenta?)null);

        var result = await _sut.GetExtractoAsync(99, new ExtractoFilter());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion

    #region GetSaldosAsync

    [Fact(DisplayName = "GetSaldos: cuenta existente devuelve saldos con acumulado")]
    public async Task GetSaldos_CuentaExistente_DevuelveSaldosConAcumulado()
    {
        _cuentaRepo.GetByIdAsync(1).Returns(CrearCuenta());
        _apunteRepo.GetSaldosMensualesAsync(1, 2026).Returns(new List<SaldoMensual>
        {
            new(1, 1000m, 200m, 5),
            new(2, 500m, 300m, 3)
        });

        var result = await _sut.GetSaldosAsync(1, 2026);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Equal(800m, result.Value![0].SaldoAcumulado);   // 1000 - 200
        Assert.Equal(1000m, result.Value![1].SaldoAcumulado);  // 800 + (500 - 300)
    }

    [Fact(DisplayName = "GetSaldos: cuenta inexistente devuelve NotFound")]
    public async Task GetSaldos_CuentaInexistente_DevuelveNotFound()
    {
        _cuentaRepo.GetByIdAsync(99).Returns((Cuenta?)null);

        var result = await _sut.GetSaldosAsync(99, 2026);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion

    #region Catálogos

    [Fact(DisplayName = "GetTiposDiario: devuelve lista")]
    public async Task GetTiposDiario_DevuelveLista()
    {
        _tipoDiarioRepo.GetAllOrdenadosAsync().Returns(new List<TipoDiario>
        {
            new() { Id = 1, Codigo = "GEN", Descripcion = "General", EmpresaId = 1 },
            new() { Id = 2, Codigo = "VEN", Descripcion = "Ventas", EmpresaId = 1 }
        });

        var result = await _sut.GetTiposDiarioAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact(DisplayName = "GetCentrosCoste: devuelve lista")]
    public async Task GetCentrosCoste_DevuelveLista()
    {
        _centroCosteRepo.GetAllOrdenadosAsync().Returns(new List<CentroCoste>
        {
            new() { Id = 1, Codigo = "CC01", Descripcion = "Centro 1", EmpresaId = 1 }
        });

        var result = await _sut.GetCentrosCostosAsync();

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    #endregion
}
