using ERPNet.Domain.Common.Values;
using ERPNet.Domain.Entities;
using ERPNet.Infrastructure.Database.Repositories;
using Xunit;

namespace ERPNet.Testing.UnitTests.Repositories;

public class RolRepositoryTests : RepositoryTestBase
{
    private readonly RolRepository _sut;

    public RolRepositoryTests()
    {
        _sut = new RolRepository(Context);
    }

    private static Rol CrearRol(int id, string nombre) => new()
    {
        Id = id,
        Nombre = nombre,
        Descripcion = "Test"
    };

    #region ExisteNombreAsync

    [Fact(DisplayName = "ExisteNombre: nombre existente devuelve true")]
    public async Task ExisteNombre_Existente_True()
    {
        Context.Roles.Add(CrearRol(1, "Admin"));
        await SaveAndClearAsync();

        Assert.True(await _sut.ExisteNombreAsync("Admin"));
    }

    [Fact(DisplayName = "ExisteNombre: nombre inexistente devuelve false")]
    public async Task ExisteNombre_Inexistente_False()
    {
        Assert.False(await _sut.ExisteNombreAsync("NoExiste"));
    }

    [Fact(DisplayName = "ExisteNombre: excluye id propio")]
    public async Task ExisteNombre_ExcluyeIdPropio()
    {
        Context.Roles.Add(CrearRol(1, "Admin"));
        await SaveAndClearAsync();

        Assert.False(await _sut.ExisteNombreAsync("Admin", excludeId: 1));
    }

    [Fact(DisplayName = "ExisteNombre: no excluye otros ids")]
    public async Task ExisteNombre_NoExcluyeOtros()
    {
        Context.Roles.Add(CrearRol(1, "Admin"));
        await SaveAndClearAsync();

        Assert.True(await _sut.ExisteNombreAsync("Admin", excludeId: 99));
    }

    #endregion

    #region GetUsuarioIdsPorRol

    [Fact(DisplayName = "GetUsuarioIdsPorRol: devuelve usuarios del rol")]
    public async Task GetUsuarioIdsPorRol_DevuelveUsuarios()
    {
        var seccion = new Seccion { Id = 1, Nombre = "IT" };
        Context.Secciones.Add(seccion);

        var emp1 = new Empleado { Id = 1, Nombre = "E1", Apellidos = "A1", DNI = Dni.From("00000001R"), SeccionId = 1 };
        var emp2 = new Empleado { Id = 2, Nombre = "E2", Apellidos = "A2", DNI = Dni.From("00000002W"), SeccionId = 1 };
        Context.Empleados.AddRange(emp1, emp2);

        var u1 = new Usuario { Id = 1, Email = Email.From("u1@t.com"), PasswordHash = "h", EmpleadoId = 1 };
        var u2 = new Usuario { Id = 2, Email = Email.From("u2@t.com"), PasswordHash = "h", EmpleadoId = 2 };
        Context.Usuarios.AddRange(u1, u2);

        var rol = CrearRol(1, "Admin");
        Context.Roles.Add(rol);

        Context.RolesUsuarios.AddRange(
            new RolUsuario { UsuarioId = 1, RolId = 1 },
            new RolUsuario { UsuarioId = 2, RolId = 1 });
        await SaveAndClearAsync();

        var result = _sut.GetUsuarioIdsPorRol(1);

        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(2, result);
    }

    [Fact(DisplayName = "GetUsuarioIdsPorRol: rol sin usuarios devuelve vacio")]
    public async Task GetUsuarioIdsPorRol_SinUsuarios_Vacio()
    {
        Context.Roles.Add(CrearRol(1, "Vacio"));
        await SaveAndClearAsync();

        var result = _sut.GetUsuarioIdsPorRol(1);

        Assert.Empty(result);
    }

    #endregion

    #region Soft Delete

    [Fact(DisplayName = "GetByIdAsync: soft-deleted devuelve null")]
    public async Task GetById_SoftDeleted_Null()
    {
        Context.Roles.Add(new Rol { Id = 1, Nombre = "Borrado", IsDeleted = true });
        await SaveAndClearAsync();

        Assert.Null(await _sut.GetByIdAsync(1));
    }

    #endregion
}
