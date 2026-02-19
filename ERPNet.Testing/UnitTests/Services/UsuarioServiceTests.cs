using ERPNet.Application.Common;
using ERPNet.Contracts.DTOs;
using ERPNet.Contracts;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Application.Mailing;
using ERPNet.Domain.Common.Values;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.UnitTests.Services;

public class UsuarioServiceTests
{
    private readonly IUsuarioRepository _repo = Substitute.For<IUsuarioRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly IMailService _mail = Substitute.For<IMailService>();
    private readonly UsuarioService _sut;

    public UsuarioServiceTests()
    {
        _sut = new UsuarioService(_repo, _uow, _cache, _mail);
    }

    private static Usuario CrearUsuario(int id = 1) => new()
    {
        Id = id,
        Email = Email.From($"user{id}@test.com"),
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1!"),
        Activo = true,
        EmpleadoId = id,
        UltimoCambioContrasena = DateTime.UtcNow
    };

    #region GetAllAsync

    [Fact(DisplayName = "GetAll: devuelve lista paginada")]
    public async Task GetAll_DevuelveListaPaginada()
    {
        var usuarios = new List<Usuario> { CrearUsuario(1), CrearUsuario(2) };
        _repo.GetPaginatedAsync(Arg.Any<PaginacionFilter>()).Returns((usuarios, 2));

        var result = await _sut.GetAllAsync(new PaginacionFilter());

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalRegistros);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetById: usuario existente devuelve OK")]
    public async Task GetById_Existente_DevuelveOk()
    {
        _repo.GetByIdAsync(1).Returns(CrearUsuario());

        var result = await _sut.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal("user1@test.com", result.Value!.Email);
    }

    [Fact(DisplayName = "GetById: usuario inexistente devuelve NotFound")]
    public async Task GetById_Inexistente_DevuelveNotFound()
    {
        _repo.GetByIdAsync(99).Returns((Usuario?)null);

        var result = await _sut.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "Create: datos validos crea usuario y envia bienvenida")]
    public async Task Create_DatosValidos_CreaUsuario()
    {
        _repo.ExisteEmailAsync("nuevo@test.com").Returns(false);
        _repo.ExisteEmpleadoAsync(5).Returns(false);

        var result = await _sut.CreateAsync(new CreateUsuarioRequest
        {
            Email = "nuevo@test.com",
            Password = "Password1!",
            EmpleadoId = 5
        });

        Assert.True(result.IsSuccess);
        await _repo.Received(1).CreateAsync(Arg.Any<Usuario>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mail.Received(1).EnviarBienvenidaAsync("nuevo@test.com", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Create: email duplicado devuelve Conflict")]
    public async Task Create_EmailDuplicado_DevuelveConflict()
    {
        _repo.ExisteEmailAsync("existe@test.com").Returns(true);

        var result = await _sut.CreateAsync(new CreateUsuarioRequest
        {
            Email = "existe@test.com",
            Password = "Password1!",
            EmpleadoId = 5
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
    }

    [Fact(DisplayName = "Create: empleado duplicado devuelve Conflict")]
    public async Task Create_EmpleadoDuplicado_DevuelveConflict()
    {
        _repo.ExisteEmailAsync("nuevo@test.com").Returns(false);
        _repo.ExisteEmpleadoAsync(5).Returns(true);

        var result = await _sut.CreateAsync(new CreateUsuarioRequest
        {
            Email = "nuevo@test.com",
            Password = "Password1!",
            EmpleadoId = 5
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = "Update: datos validos actualiza usuario")]
    public async Task Update_DatosValidos_ActualizaUsuario()
    {
        _repo.GetByIdAsync(1).Returns(CrearUsuario());

        var result = await _sut.UpdateAsync(1, new UpdateUsuarioRequest { Activo = false });

        Assert.True(result.IsSuccess);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Update: email duplicado devuelve Conflict")]
    public async Task Update_EmailDuplicado_DevuelveConflict()
    {
        _repo.GetByIdAsync(1).Returns(CrearUsuario());
        _repo.ExisteEmailAsync("otro@test.com", 1).Returns(true);

        var result = await _sut.UpdateAsync(1, new UpdateUsuarioRequest { Email = "otro@test.com" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
    }

    [Fact(DisplayName = "Update: inexistente devuelve NotFound")]
    public async Task Update_Inexistente_DevuelveNotFound()
    {
        _repo.GetByIdAsync(99).Returns((Usuario?)null);

        var result = await _sut.UpdateAsync(99, new UpdateUsuarioRequest { Activo = false });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = "Delete: usuario existente lo elimina")]
    public async Task Delete_Existente_Elimina()
    {
        var usuario = CrearUsuario();
        _repo.GetByIdAsync(1).Returns(usuario);

        var result = await _sut.DeleteAsync(1);

        Assert.True(result.IsSuccess);
        _repo.Received(1).Delete(usuario);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Delete: inexistente devuelve NotFound")]
    public async Task Delete_Inexistente_DevuelveNotFound()
    {
        _repo.GetByIdAsync(99).Returns((Usuario?)null);

        var result = await _sut.DeleteAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion

    #region CambiarContrasenaAsync

    [Fact(DisplayName = "CambiarContrasena: contrasena correcta la cambia")]
    public async Task CambiarContrasena_Correcta_Cambia()
    {
        var usuario = CrearUsuario();
        _repo.GetByIdAsync(1).Returns(usuario);

        var result = await _sut.CambiarContrasenaAsync(1, new CambiarContrasenaRequest
        {
            ContrasenaActual = "Password1!",
            NuevaContrasena = "NuevaPass1!",
            ConfirmarContrasena = "NuevaPass1!"
        });

        Assert.True(result.IsSuccess);
        Assert.True(BCrypt.Net.BCrypt.Verify("NuevaPass1!", usuario.PasswordHash));
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "CambiarContrasena: contrasena incorrecta devuelve Validation")]
    public async Task CambiarContrasena_Incorrecta_DevuelveValidation()
    {
        _repo.GetByIdAsync(1).Returns(CrearUsuario());

        var result = await _sut.CambiarContrasenaAsync(1, new CambiarContrasenaRequest
        {
            ContrasenaActual = "WrongPass!",
            NuevaContrasena = "NuevaPass1!",
            ConfirmarContrasena = "NuevaPass1!"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
    }

    [Fact(DisplayName = "CambiarContrasena: usuario inexistente devuelve NotFound")]
    public async Task CambiarContrasena_Inexistente_DevuelveNotFound()
    {
        _repo.GetByIdAsync(99).Returns((Usuario?)null);

        var result = await _sut.CambiarContrasenaAsync(99, new CambiarContrasenaRequest
        {
            ContrasenaActual = "X",
            NuevaContrasena = "Y",
            ConfirmarContrasena = "Y"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion

    #region ResetearContrasenaAsync

    [Fact(DisplayName = "ResetearContrasena: usuario existente resetea")]
    public async Task ResetearContrasena_Existente_Resetea()
    {
        var usuario = CrearUsuario();
        _repo.GetByIdAsync(1).Returns(usuario);

        var result = await _sut.ResetearContrasenaAsync(1, new ResetearContrasenaRequest
        {
            NuevaContrasena = "Reset123!",
            ConfirmarContrasena = "Reset123!"
        });

        Assert.True(result.IsSuccess);
        Assert.True(BCrypt.Net.BCrypt.Verify("Reset123!", usuario.PasswordHash));
    }

    [Fact(DisplayName = "ResetearContrasena: inexistente devuelve NotFound")]
    public async Task ResetearContrasena_Inexistente_DevuelveNotFound()
    {
        _repo.GetByIdAsync(99).Returns((Usuario?)null);

        var result = await _sut.ResetearContrasenaAsync(99, new ResetearContrasenaRequest
        {
            NuevaContrasena = "X",
            ConfirmarContrasena = "X"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion

    #region GetRolesAsync

    [Fact(DisplayName = "GetRoles: usuario existente devuelve rolIds")]
    public async Task GetRoles_Existente_DevuelveRolIds()
    {
        _repo.GetByIdAsync(1).Returns(CrearUsuario());
        _repo.GetRolIdsAsync(1).Returns([1, 3]);

        var result = await _sut.GetRolesAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact(DisplayName = "GetRoles: usuario inexistente devuelve NotFound")]
    public async Task GetRoles_Inexistente_DevuelveNotFound()
    {
        _repo.GetByIdAsync(99).Returns((Usuario?)null);

        var result = await _sut.GetRolesAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion

    #region AsignarRolesAsync

    [Fact(DisplayName = "AsignarRoles: usuario existente sincroniza roles")]
    public async Task AsignarRoles_Existente_SincronizaRoles()
    {
        _repo.GetByIdAsync(1).Returns(CrearUsuario());

        var result = await _sut.AsignarRolesAsync(1, new AsignarRolesRequest { RolIds = [1, 2] });

        Assert.True(result.IsSuccess);
        await _repo.Received(1).SincronizarRolesAsync(1, Arg.Is<List<int>>(l => l.Contains(1) && l.Contains(2)));
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "AsignarRoles: usuario inexistente devuelve NotFound")]
    public async Task AsignarRoles_Inexistente_DevuelveNotFound()
    {
        _repo.GetByIdAsync(99).Returns((Usuario?)null);

        var result = await _sut.AsignarRolesAsync(99, new AsignarRolesRequest { RolIds = [1] });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion

    #region Invalidación de caché

    [Fact(DisplayName = "Update: invalida cache del usuario")]
    public async Task Update_InvalidaCache()
    {
        _repo.GetByIdAsync(1).Returns(CrearUsuario());

        await _sut.UpdateAsync(1, new UpdateUsuarioRequest { Activo = false });

        _cache.Received(1).Remove("usuario:1");
    }

    [Fact(DisplayName = "Delete: invalida cache del usuario")]
    public async Task Delete_InvalidaCache()
    {
        _repo.GetByIdAsync(1).Returns(CrearUsuario());

        await _sut.DeleteAsync(1);

        _cache.Received(1).Remove("usuario:1");
    }

    [Fact(DisplayName = "CambiarContrasena: invalida cache del usuario")]
    public async Task CambiarContrasena_InvalidaCache()
    {
        _repo.GetByIdAsync(1).Returns(CrearUsuario());

        await _sut.CambiarContrasenaAsync(1, new CambiarContrasenaRequest
        {
            ContrasenaActual = "Password1!",
            NuevaContrasena = "NuevaPass1!",
            ConfirmarContrasena = "NuevaPass1!"
        });

        _cache.Received(1).Remove("usuario:1");
    }

    [Fact(DisplayName = "ResetearContrasena: invalida cache del usuario")]
    public async Task ResetearContrasena_InvalidaCache()
    {
        _repo.GetByIdAsync(1).Returns(CrearUsuario());

        await _sut.ResetearContrasenaAsync(1, new ResetearContrasenaRequest
        {
            NuevaContrasena = "Reset123!",
            ConfirmarContrasena = "Reset123!"
        });

        _cache.Received(1).Remove("usuario:1");
    }

    [Fact(DisplayName = "AsignarRoles: invalida cache del usuario")]
    public async Task AsignarRoles_InvalidaCache()
    {
        _repo.GetByIdAsync(1).Returns(CrearUsuario());

        await _sut.AsignarRolesAsync(1, new AsignarRolesRequest { RolIds = [1, 2] });

        _cache.Received(1).Remove("usuario:1");
    }

    [Fact(DisplayName = "Create: no invalida cache")]
    public async Task Create_NoInvalidaCache()
    {
        _repo.ExisteEmailAsync("nuevo@test.com").Returns(false);
        _repo.ExisteEmpleadoAsync(5).Returns(false);

        await _sut.CreateAsync(new CreateUsuarioRequest
        {
            Email = "nuevo@test.com",
            Password = "Password1!",
            EmpleadoId = 5
        });

        _cache.DidNotReceive().Remove(Arg.Any<string>());
    }

    #endregion
}
