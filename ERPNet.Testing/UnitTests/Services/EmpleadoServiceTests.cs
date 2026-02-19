using ERPNet.Application.Auth;
using ERPNet.Application.Auth.Interfaces;
using ERPNet.Application.Common;
using ERPNet.Contracts.DTOs;
using ERPNet.Contracts;
using ERPNet.Domain.Common.Values;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.UnitTests.Services;

public class EmpleadoServiceTests
{
    private readonly IEmpleadoRepository _repo = Substitute.For<IEmpleadoRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserProvider _currentUser = Substitute.For<ICurrentUserProvider>();
    private readonly EmpleadoService _sut;

    public EmpleadoServiceTests()
    {
        _sut = new EmpleadoService(_repo, _uow, _currentUser);
    }

    private void SetupUsuario(int empleadoId = 1, int seccionId = 1, Alcance alcance = Alcance.Global)
    {
        var permisos = new List<PermisoUsuario>
        {
            new(RecursoCodigo.Empleados, true, true, true, alcance)
        };
        _currentUser.Current.Returns(new UsuarioContext(1, "test@test.com", empleadoId, seccionId, permisos, [1], false));
    }

    private static Empleado CrearEmpleado(int id = 1, int seccionId = 1) => new()
    {
        Id = id,
        Nombre = "Juan",
        Apellidos = "García",
        DNI = Dni.From("12345678Z"),
        Activo = true,
        SeccionId = seccionId
    };

    #region GetAllAsync

    [Fact(DisplayName = "GetAll: Alcance Global devuelve todos")]
    public async Task GetAll_Global_DevuelveTodos()
    {
        SetupUsuario(alcance: Alcance.Global);
        var empleados = new List<Empleado> { CrearEmpleado(1), CrearEmpleado(2) };
        _repo.GetPaginatedAsync(Arg.Any<PaginacionFilter>(), Alcance.Global, 1, 1)
            .Returns((empleados, 2));

        var result = await _sut.GetAllAsync(new PaginacionFilter());

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalRegistros);
    }

    [Fact(DisplayName = "GetAll: Alcance Seccion filtra por seccion")]
    public async Task GetAll_Seccion_FiltraPorSeccion()
    {
        SetupUsuario(seccionId: 3, alcance: Alcance.Seccion);
        _repo.GetPaginatedAsync(Arg.Any<PaginacionFilter>(), Alcance.Seccion, 1, 3)
            .Returns((new List<Empleado> { CrearEmpleado(1, seccionId: 3) }, 1));

        var result = await _sut.GetAllAsync(new PaginacionFilter());

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.TotalRegistros);
    }

    [Fact(DisplayName = "GetAll: Alcance Propio filtra por empleadoId")]
    public async Task GetAll_Propio_FiltraPorEmpleadoId()
    {
        SetupUsuario(empleadoId: 5, alcance: Alcance.Propio);
        _repo.GetPaginatedAsync(Arg.Any<PaginacionFilter>(), Alcance.Propio, 5, 1)
            .Returns((new List<Empleado> { CrearEmpleado(5) }, 1));

        var result = await _sut.GetAllAsync(new PaginacionFilter());

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetById: empleado existente con acceso devuelve OK")]
    public async Task GetById_ConAcceso_DevuelveOk()
    {
        SetupUsuario(alcance: Alcance.Global);
        _repo.GetByIdAsync(1).Returns(CrearEmpleado());

        var result = await _sut.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal("Juan", result.Value!.Nombre);
    }

    [Fact(DisplayName = "GetById: empleado inexistente devuelve NotFound")]
    public async Task GetById_Inexistente_DevuelveNotFound()
    {
        SetupUsuario();
        _repo.GetByIdAsync(99).Returns((Empleado?)null);

        var result = await _sut.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact(DisplayName = "GetById: Alcance Propio sin acceso devuelve Forbidden")]
    public async Task GetById_PropioSinAcceso_DevuelveForbidden()
    {
        SetupUsuario(empleadoId: 5, alcance: Alcance.Propio);
        _repo.GetByIdAsync(1).Returns(CrearEmpleado(1));

        var result = await _sut.GetByIdAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
    }

    [Fact(DisplayName = "GetById: Alcance Seccion otra seccion devuelve Forbidden")]
    public async Task GetById_SeccionDistinta_DevuelveForbidden()
    {
        SetupUsuario(seccionId: 1, alcance: Alcance.Seccion);
        _repo.GetByIdAsync(1).Returns(CrearEmpleado(1, seccionId: 99));

        var result = await _sut.GetByIdAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
    }

    [Fact(DisplayName = "GetById: Alcance Seccion misma seccion devuelve OK")]
    public async Task GetById_MismaSeccion_DevuelveOk()
    {
        SetupUsuario(seccionId: 3, alcance: Alcance.Seccion);
        _repo.GetByIdAsync(1).Returns(CrearEmpleado(1, seccionId: 3));

        var result = await _sut.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
    }

    #endregion

    #region GetMeAsync

    [Fact(DisplayName = "GetMe: devuelve el empleado del usuario autenticado")]
    public async Task GetMe_DevuelveEmpleadoPropio()
    {
        SetupUsuario(empleadoId: 7);
        _repo.GetByIdAsync(7).Returns(CrearEmpleado(7));

        var result = await _sut.GetMeAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(7, result.Value!.Id);
    }

    [Fact(DisplayName = "GetMe: empleado inexistente devuelve NotFound")]
    public async Task GetMe_EmpleadoInexistente_DevuelveNotFound()
    {
        SetupUsuario(empleadoId: 999);
        _repo.GetByIdAsync(999).Returns((Empleado?)null);

        var result = await _sut.GetMeAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "Create: datos validos crea empleado")]
    public async Task Create_DatosValidos_CreaEmpleado()
    {
        SetupUsuario();
        _repo.ExisteDniAsync("12345678Z").Returns(false);

        var request = new CreateEmpleadoRequest
        {
            Nombre = "Ana",
            Apellidos = "López",
            Dni = "12345678Z",
            SeccionId = 1
        };

        var result = await _sut.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("Ana", result.Value!.Nombre);
        await _repo.Received(1).CreateAsync(Arg.Any<Empleado>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Create: DNI duplicado devuelve Conflict")]
    public async Task Create_DniDuplicado_DevuelveConflict()
    {
        SetupUsuario();
        _repo.ExisteDniAsync("12345678Z").Returns(true);

        var request = new CreateEmpleadoRequest
        {
            Nombre = "Ana",
            Apellidos = "López",
            Dni = "12345678Z",
            SeccionId = 1
        };

        var result = await _sut.CreateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = "Update: datos validos actualiza empleado")]
    public async Task Update_DatosValidos_ActualizaEmpleado()
    {
        SetupUsuario(alcance: Alcance.Global);
        var empleado = CrearEmpleado();
        _repo.GetByIdAsync(1).Returns(empleado);

        var result = await _sut.UpdateAsync(1, new UpdateEmpleadoRequest { Nombre = "Pedro" });

        Assert.True(result.IsSuccess);
        Assert.Equal("Pedro", empleado.Nombre);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Update: sin acceso devuelve Forbidden")]
    public async Task Update_SinAcceso_DevuelveForbidden()
    {
        SetupUsuario(empleadoId: 5, alcance: Alcance.Propio);
        _repo.GetByIdAsync(1).Returns(CrearEmpleado(1));

        var result = await _sut.UpdateAsync(1, new UpdateEmpleadoRequest { Nombre = "Pedro" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
    }

    [Fact(DisplayName = "Update: DNI duplicado devuelve Conflict")]
    public async Task Update_DniDuplicado_DevuelveConflict()
    {
        SetupUsuario(alcance: Alcance.Global);
        _repo.GetByIdAsync(1).Returns(CrearEmpleado());
        _repo.ExisteDniAsync("99999999A", 1).Returns(true);

        var result = await _sut.UpdateAsync(1, new UpdateEmpleadoRequest { Dni = "99999999A" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
    }

    [Fact(DisplayName = "Update: inexistente devuelve NotFound")]
    public async Task Update_Inexistente_DevuelveNotFound()
    {
        SetupUsuario();
        _repo.GetByIdAsync(99).Returns((Empleado?)null);

        var result = await _sut.UpdateAsync(99, new UpdateEmpleadoRequest { Nombre = "X" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = "Delete: con acceso elimina empleado")]
    public async Task Delete_ConAcceso_Elimina()
    {
        SetupUsuario(alcance: Alcance.Global);
        var empleado = CrearEmpleado();
        _repo.GetByIdAsync(1).Returns(empleado);

        var result = await _sut.DeleteAsync(1);

        Assert.True(result.IsSuccess);
        _repo.Received(1).Delete(empleado);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Delete: sin acceso devuelve Forbidden")]
    public async Task Delete_SinAcceso_DevuelveForbidden()
    {
        SetupUsuario(empleadoId: 5, alcance: Alcance.Propio);
        _repo.GetByIdAsync(1).Returns(CrearEmpleado(1));

        var result = await _sut.DeleteAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
    }

    [Fact(DisplayName = "Delete: inexistente devuelve NotFound")]
    public async Task Delete_Inexistente_DevuelveNotFound()
    {
        SetupUsuario();
        _repo.GetByIdAsync(99).Returns((Empleado?)null);

        var result = await _sut.DeleteAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion
}
