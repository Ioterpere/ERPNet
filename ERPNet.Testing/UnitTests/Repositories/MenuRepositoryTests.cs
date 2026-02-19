using ERPNet.Domain.Common.Values;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
using ERPNet.Infrastructure.Database.Repositories;
using Xunit;

namespace ERPNet.Testing.UnitTests.Repositories;

public class MenuRepositoryTests : RepositoryTestBase
{
    private readonly MenuRepository _sut;

    public MenuRepositoryTests()
    {
        _sut = new MenuRepository(Context);
        SeedRoles();
    }

    private void SeedRoles()
    {
        Context.Roles.AddRange(
            new Rol { Id = 1, Nombre = "Admin" },
            new Rol { Id = 2, Nombre = "Editor" });
        Context.SaveChanges();
        Context.ChangeTracker.Clear();
    }

    private static Menu CrearMenu(int id, string nombre, Plataforma plataforma = Plataforma.WebBlazor, int? padreId = null) => new()
    {
        Id = id,
        Nombre = nombre,
        Orden = id,
        Plataforma = plataforma,
        MenuPadreId = padreId
    };

    #region GetMenusVisiblesAsync

    [Fact(DisplayName = "GetMenusVisibles: devuelve menus filtrando por plataforma")]
    public async Task GetMenusVisibles_FiltraPorPlataforma()
    {
        Context.Menus.AddRange(
            CrearMenu(1, "MenuA", Plataforma.WebBlazor),
            CrearMenu(2, "MenuB", Plataforma.WebBlazor));
        await SaveAndClearAsync();

        var result = await _sut.GetMenusVisiblesAsync(Plataforma.WebBlazor, [1]);

        Assert.Equal(2, result.Count);
    }

    [Fact(DisplayName = "GetMenusVisibles: menu sin roles es visible para todos")]
    public async Task GetMenusVisibles_SinRoles_VisibleParaTodos()
    {
        Context.Menus.Add(CrearMenu(1, "Publico"));
        await SaveAndClearAsync();

        var result = await _sut.GetMenusVisiblesAsync(Plataforma.WebBlazor, [99]);

        Assert.Single(result);
    }

    [Fact(DisplayName = "GetMenusVisibles: menu con roles filtra por rol del usuario")]
    public async Task GetMenusVisibles_ConRoles_Filtra()
    {
        Context.Menus.AddRange(CrearMenu(1, "AdminMenu"), CrearMenu(2, "EditorMenu"));
        Context.MenusRoles.AddRange(
            new MenuRol { MenuId = 1, RolId = 1 },
            new MenuRol { MenuId = 2, RolId = 2 });
        await SaveAndClearAsync();

        var result = await _sut.GetMenusVisiblesAsync(Plataforma.WebBlazor, [1]);

        Assert.Single(result);
        Assert.Equal("AdminMenu", result[0].Nombre);
    }

    [Fact(DisplayName = "GetMenusVisibles: solo devuelve menus raiz")]
    public async Task GetMenusVisibles_SoloRaiz()
    {
        Context.Menus.AddRange(
            CrearMenu(1, "Padre"),
            CrearMenu(2, "Hijo", padreId: 1));
        await SaveAndClearAsync();

        var result = await _sut.GetMenusVisiblesAsync(Plataforma.WebBlazor, [1]);

        Assert.Single(result);
        Assert.Equal("Padre", result[0].Nombre);
    }

    [Fact(DisplayName = "GetMenusVisibles: incluye submenus ordenados")]
    public async Task GetMenusVisibles_IncluyeSubMenus()
    {
        Context.Menus.AddRange(
            CrearMenu(1, "Padre"),
            CrearMenu(3, "HijoB", padreId: 1),
            CrearMenu(2, "HijoA", padreId: 1));
        await SaveAndClearAsync();

        var result = await _sut.GetMenusVisiblesAsync(Plataforma.WebBlazor, [1]);

        Assert.Single(result);
        Assert.Equal(2, result[0].SubMenus.Count);
        Assert.Equal("HijoA", result[0].SubMenus.First().Nombre);
    }

    #endregion

    #region GetRolIdsAsync / SincronizarRolesAsync

    [Fact(DisplayName = "GetRolIds: devuelve roles del menu")]
    public async Task GetRolIds_DevuelveRoles()
    {
        Context.Menus.Add(CrearMenu(1, "Menu"));
        Context.MenusRoles.AddRange(
            new MenuRol { MenuId = 1, RolId = 1 },
            new MenuRol { MenuId = 1, RolId = 2 });
        await SaveAndClearAsync();

        var result = await _sut.GetRolIdsAsync(1);

        Assert.Equal(2, result.Count);
    }

    [Fact(DisplayName = "SincronizarRoles: agrega y elimina roles")]
    public async Task SincronizarRoles_AgregaYElimina()
    {
        Context.Menus.Add(CrearMenu(1, "Menu"));
        Context.MenusRoles.Add(new MenuRol { MenuId = 1, RolId = 1 });
        await SaveAndClearAsync();

        await _sut.SincronizarRolesAsync(1, [2]);
        await SaveAndClearAsync();

        var result = await _sut.GetRolIdsAsync(1);
        Assert.Single(result);
        Assert.Contains(2, result);
    }

    #endregion

    #region GetUsuarioIdsPorRolesAsync

    [Fact(DisplayName = "GetUsuarioIdsPorRoles: devuelve usuarios distinctos")]
    public async Task GetUsuarioIdsPorRoles_DevuelveDistinctos()
    {
        var seccion = new Seccion { Id = 1, Nombre = "IT" };
        Context.Secciones.Add(seccion);
        Context.Empleados.AddRange(
            new Empleado { Id = 1, Nombre = "E1", Apellidos = "A1", DNI = Dni.From("00000001R"), SeccionId = 1 },
            new Empleado { Id = 2, Nombre = "E2", Apellidos = "A2", DNI = Dni.From("00000002W"), SeccionId = 1 });
        Context.Usuarios.AddRange(
            new Usuario { Id = 1, Email = Email.From("u1@t.com"), PasswordHash = "h", EmpleadoId = 1 },
            new Usuario { Id = 2, Email = Email.From("u2@t.com"), PasswordHash = "h", EmpleadoId = 2 });

        // Usuario 1 tiene ambos roles
        Context.RolesUsuarios.AddRange(
            new RolUsuario { UsuarioId = 1, RolId = 1 },
            new RolUsuario { UsuarioId = 1, RolId = 2 },
            new RolUsuario { UsuarioId = 2, RolId = 1 });
        await SaveAndClearAsync();

        var result = await _sut.GetUsuarioIdsPorRolesAsync([1, 2]);

        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(2, result);
    }

    #endregion
}
