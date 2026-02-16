using ERPNet.Domain.Common.Values;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Filters;
using ERPNet.Infrastructure.Database.Repositories;
using Xunit;

namespace ERPNet.Testing.UnitTests.Repositories;

public class UsuarioRepositoryTests : RepositoryTestBase
{
    private readonly UsuarioRepository _sut;

    public UsuarioRepositoryTests()
    {
        _sut = new UsuarioRepository(Context);
        SeedBase();
    }

    private void SeedBase()
    {
        Context.Secciones.Add(new Seccion { Id = 1, Nombre = "IT" });
        Context.Empleados.AddRange(
            new Empleado { Id = 1, Nombre = "E1", Apellidos = "A1", DNI = Dni.From("00000001R"), SeccionId = 1 },
            new Empleado { Id = 2, Nombre = "E2", Apellidos = "A2", DNI = Dni.From("00000002W"), SeccionId = 1 },
            new Empleado { Id = 3, Nombre = "E3", Apellidos = "A3", DNI = Dni.From("00000003A"), SeccionId = 1 });
        Context.SaveChanges();
        Context.ChangeTracker.Clear();
    }

    public static Usuario Crear(int id, string email, int empleadoId, bool activo = true) => new()
    {
        Id = id,
        Email = Email.From(email),
        PasswordHash = "hash",
        Activo = activo,
        EmpleadoId = empleadoId,
        UltimoCambioContrasena = DateTime.UtcNow
    };

    #region ExisteEmailAsync

    [Fact(DisplayName = "ExisteEmail: existente devuelve true")]
    public async Task ExisteEmail_Existente_True()
    {
        Context.Usuarios.Add(Crear(1, "admin@test.com", 1));
        await SaveAndClearAsync();

        Assert.True(await _sut.ExisteEmailAsync("admin@test.com"));
    }

    [Fact(DisplayName = "ExisteEmail: inexistente devuelve false")]
    public async Task ExisteEmail_Inexistente_False()
    {
        Assert.False(await _sut.ExisteEmailAsync("nope@test.com"));
    }

    [Fact(DisplayName = "ExisteEmail: excluye id propio")]
    public async Task ExisteEmail_ExcluyeIdPropio()
    {
        Context.Usuarios.Add(Crear(1, "admin@test.com", 1));
        await SaveAndClearAsync();

        Assert.False(await _sut.ExisteEmailAsync("admin@test.com", excluirId: 1));
    }

    #endregion

    #region ExisteEmpleadoAsync

    [Fact(DisplayName = "ExisteEmpleado: empleado vinculado devuelve true")]
    public async Task ExisteEmpleado_Vinculado_True()
    {
        Context.Usuarios.Add(Crear(1, "admin@test.com", 1));
        await SaveAndClearAsync();

        Assert.True(await _sut.ExisteEmpleadoAsync(1));
    }

    [Fact(DisplayName = "ExisteEmpleado: empleado libre devuelve false")]
    public async Task ExisteEmpleado_Libre_False()
    {
        Assert.False(await _sut.ExisteEmpleadoAsync(99));
    }

    [Fact(DisplayName = "ExisteEmpleado: excluye id propio")]
    public async Task ExisteEmpleado_ExcluyeIdPropio()
    {
        Context.Usuarios.Add(Crear(1, "admin@test.com", 1));
        await SaveAndClearAsync();

        Assert.False(await _sut.ExisteEmpleadoAsync(1, excluirId: 1));
    }

    #endregion

    #region GetByEmailAsync

    [Fact(DisplayName = "GetByEmail: existente devuelve usuario con empleado")]
    public async Task GetByEmail_Existente_DevuelveConEmpleado()
    {
        Context.Usuarios.Add(Crear(1, "admin@test.com", 1));
        await SaveAndClearAsync();

        var result = await _sut.GetByEmailAsync("admin@test.com");

        Assert.NotNull(result);
        Assert.NotNull(result.Empleado);
        Assert.Equal("E1", result.Empleado.Nombre);
    }

    [Fact(DisplayName = "GetByEmail: inexistente devuelve null")]
    public async Task GetByEmail_Inexistente_Null()
    {
        Assert.Null(await _sut.GetByEmailAsync("nope@test.com"));
    }

    #endregion

    #region GetRolIdsAsync / SincronizarRolesAsync

    [Fact(DisplayName = "GetRolIds: devuelve los roles del usuario")]
    public async Task GetRolIds_DevuelveRoles()
    {
        Context.Usuarios.Add(Crear(1, "u@t.com", 1));
        Context.Roles.AddRange(
            new Rol { Id = 1, Nombre = "Admin" },
            new Rol { Id = 2, Nombre = "Editor" });
        Context.RolesUsuarios.AddRange(
            new RolUsuario { UsuarioId = 1, RolId = 1 },
            new RolUsuario { UsuarioId = 1, RolId = 2 });
        await SaveAndClearAsync();

        var result = await _sut.GetRolIdsAsync(1);

        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(2, result);
    }

    [Fact(DisplayName = "SincronizarRoles: agrega nuevos y elimina viejos")]
    public async Task SincronizarRoles_AgregaYElimina()
    {
        Context.Usuarios.Add(Crear(1, "u@t.com", 1));
        Context.Roles.AddRange(
            new Rol { Id = 1, Nombre = "Admin" },
            new Rol { Id = 2, Nombre = "Editor" },
            new Rol { Id = 3, Nombre = "Viewer" });
        Context.RolesUsuarios.AddRange(
            new RolUsuario { UsuarioId = 1, RolId = 1 },
            new RolUsuario { UsuarioId = 1, RolId = 2 });
        await SaveAndClearAsync();

        // Mantiene 2, quita 1, agrega 3
        await _sut.SincronizarRolesAsync(1, [2, 3]);
        await SaveAndClearAsync();

        var result = await _sut.GetRolIdsAsync(1);

        Assert.Equal(2, result.Count);
        Assert.Contains(2, result);
        Assert.Contains(3, result);
        Assert.DoesNotContain(1, result);
    }

    [Fact(DisplayName = "SincronizarRoles: lista vacia elimina todos")]
    public async Task SincronizarRoles_VaciaEliminaTodos()
    {
        Context.Usuarios.Add(Crear(1, "u@t.com", 1));
        Context.Roles.Add(new Rol { Id = 1, Nombre = "Admin" });
        Context.RolesUsuarios.Add(new RolUsuario { UsuarioId = 1, RolId = 1 });
        await SaveAndClearAsync();

        await _sut.SincronizarRolesAsync(1, []);
        await SaveAndClearAsync();

        var result = await _sut.GetRolIdsAsync(1);
        Assert.Empty(result);
    }

    #endregion

    #region GetByIdConPermisosAsync

    [Fact(DisplayName = "GetByIdConPermisos: carga roles y permisos")]
    public async Task GetByIdConPermisos_CargaRolesYPermisos()
    {
        Context.Usuarios.Add(Crear(1, "u@t.com", 1));
        var rol = new Rol { Id = 1, Nombre = "Admin" };
        Context.Roles.Add(rol);
        Context.RolesUsuarios.Add(new RolUsuario { UsuarioId = 1, RolId = 1 });
        Context.PermisosRolRecurso.Add(new PermisoRolRecurso
        {
            RolId = 1, RecursoId = 1, CanCreate = true, CanEdit = true, CanDelete = false
        });
        await SaveAndClearAsync();

        var result = await _sut.GetByIdConPermisosAsync(1);

        Assert.NotNull(result);
        Assert.NotNull(result.Empleado);
        Assert.Single(result.RolesUsuarios);
        var permiso = result.RolesUsuarios.First().Rol.PermisosRolRecurso.First();
        Assert.True(permiso.CanCreate);
        Assert.True(permiso.CanEdit);
        Assert.False(permiso.CanDelete);
    }

    [Fact(DisplayName = "GetByIdConPermisos: inexistente devuelve null")]
    public async Task GetByIdConPermisos_Inexistente_Null()
    {
        Assert.Null(await _sut.GetByIdConPermisosAsync(99));
    }

    #endregion

    #region Paginacion base

    [Fact(DisplayName = "GetPaginated: ordena por Id descendente")]
    public async Task GetPaginated_OrdenDescendente()
    {
        Context.Usuarios.AddRange(
            Crear(1, "a@t.com", 1),
            Crear(2, "b@t.com", 2),
            Crear(3, "c@t.com", 3));
        await SaveAndClearAsync();

        var (items, _) = await _sut.GetPaginatedAsync(new PaginacionFilter { PorPagina = 2 });

        Assert.Equal(3, items[0].Id);
        Assert.Equal(2, items[1].Id);
    }

    #endregion
}
