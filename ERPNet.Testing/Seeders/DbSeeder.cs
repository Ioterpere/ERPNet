using Bogus;
using ERPNet.Infrastructure.Database.Context;
using ERPNet.Domain.Common.Values;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace ERPNet.Testing.Seeders;

public class DbSeeder(ITestOutputHelper output)
{
    private static readonly string ConnectionString = new ConfigurationBuilder()
        .SetBasePath(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "ERPNet.Api")))
        .AddJsonFile("appsettings.json")
        .Build()
        .GetConnectionString("DefaultConnection")!;

    [Fact(DisplayName = "Seed: Migrar BD y crear datos iniciales")]
    public async Task SeedDatabase()
    {
        var options = new DbContextOptionsBuilder<ERPNetDbContext>()
            .UseSqlServer(ConnectionString, sql => sql.EnableRetryOnFailure())
            .Options;

        using var context = new ERPNetDbContext(options);

        output.WriteLine("Eliminando BD existente...");
        await context.Database.EnsureDeletedAsync();
        output.WriteLine("BD eliminada.");

        output.WriteLine("Aplicando migraciones...");
        await context.Database.MigrateAsync();
        output.WriteLine("Migraciones aplicadas.");

        output.WriteLine("Insertando datos...");

        PermisoRolRecurso Permiso(int rolId, RecursoCodigo recurso, bool c, bool e, bool d, Alcance alcance) =>
            new() { RolId = rolId, RecursoId = (int)recurso, CanCreate = c, CanEdit = e, CanDelete = d, Alcance = alcance };

        // ── Secciones ──────────────────────────────────────────────────────

        var secciones = new[]
        {
            "Recursos Humanos", "Calidad", "Mantenimiento", "Produccion",
            "Contabilidad", "Comercial", "Carnicos", "Pescados", "Paletizado", "Informatica",
        }.Select(n => new Seccion { Nombre = n, CreatedAt = DateTime.UtcNow }).ToArray();

        context.Secciones.AddRange(secciones);
        await context.SaveChangesAsync();

        // ── Empleado admin ─────────────────────────────────────────────────

        var seccionInformatica = secciones.Single(s => s.Nombre == "Informatica");

        var empAdmin = new Empleado
        {
            Nombre = "Admin", Apellidos = "Sistema", DNI = Dni.From("00000000T"),
            Activo = true, SeccionId = seccionInformatica.Id, CreatedAt = DateTime.UtcNow,
        };
        context.Empleados.Add(empAdmin);
        await context.SaveChangesAsync();

        // ── Menus padres ───────────────────────────────────────────────────

        var menuAdmin = new Menu { Nombre = "Administracion", Orden = 1, Plataforma = Plataforma.Web, CreatedAt = DateTime.UtcNow };
        var menuRRHH = new Menu { Nombre = "Recursos Humanos", Orden = 2, Plataforma = Plataforma.Web, CreatedAt = DateTime.UtcNow };
        var menuMantenimiento = new Menu { Nombre = "Mantenimiento", Orden = 3, Plataforma = Plataforma.Web, CreatedAt = DateTime.UtcNow };
        var menuProduccion = new Menu { Nombre = "Produccion y Calidad", Orden = 4, Plataforma = Plataforma.Web, CreatedAt = DateTime.UtcNow };
        var menuComercial = new Menu { Nombre = "Gestion Comercial", Orden = 5, Plataforma = Plataforma.Web, CreatedAt = DateTime.UtcNow };

        context.Menus.AddRange(menuAdmin, menuRRHH, menuMantenimiento, menuProduccion, menuComercial);
        await context.SaveChangesAsync();

        // ── Menus hijos ────────────────────────────────────────────────────

        // 1. Administracion
        var menuUsuarios = new Menu { Nombre = "Usuarios", Path = "/usuarios", Orden = 1, MenuPadreId = menuAdmin.Id, Plataforma = Plataforma.Web, CreatedAt = DateTime.UtcNow };
        var menuRoles = new Menu { Nombre = "Roles", Path = "/roles", Orden = 2, MenuPadreId = menuAdmin.Id, Plataforma = Plataforma.Web, CreatedAt = DateTime.UtcNow };
        var menuMenus = new Menu { Nombre = "Menu", Path = "/menus", Orden = 3, MenuPadreId = menuAdmin.Id, Plataforma = Plataforma.Web, CreatedAt = DateTime.UtcNow };

        // 2. Recursos Humanos
        var menuEmpleados = new Menu { Nombre = "Empleados", Path = "/empleados", Orden = 1, MenuPadreId = menuRRHH.Id, Plataforma = Plataforma.Web, CreatedAt = DateTime.UtcNow };
        var menuJornadas = new Menu { Nombre = "Gestion de Jornadas", Path = "/jornadas", Orden = 2, MenuPadreId = menuRRHH.Id, Plataforma = Plataforma.Web, CreatedAt = DateTime.UtcNow };
        var menuIncidencias = new Menu { Nombre = "Incidencias de Marcaje", Path = "/incidencias-marcaje", Orden = 3, MenuPadreId = menuRRHH.Id, Plataforma = Plataforma.Web, CreatedAt = DateTime.UtcNow };
        var menuValidar = new Menu { Nombre = "Validar Gestiones", Path = "/validar-gestiones", Orden = 4, MenuPadreId = menuRRHH.Id, Plataforma = Plataforma.Web, CreatedAt = DateTime.UtcNow };

        // 3. Mantenimiento
        var menuMaquinaria = new Menu { Nombre = "Maquinaria", Path = "/maquinaria", Orden = 1, MenuPadreId = menuMantenimiento.Id, Plataforma = Plataforma.Web, CreatedAt = DateTime.UtcNow };
        var menuOrdenesMant = new Menu { Nombre = "Ordenes de Mantenimiento", Path = "/ordenes-mantenimiento", Orden = 2, MenuPadreId = menuMantenimiento.Id, Plataforma = Plataforma.Web, CreatedAt = DateTime.UtcNow };
        var menuTareasMant = new Menu { Nombre = "Tareas de Mantenimiento", Path = "/tareas-mantenimiento", Orden = 3, MenuPadreId = menuMantenimiento.Id, Plataforma = Plataforma.Web, CreatedAt = DateTime.UtcNow };
        var menuReportarAveria = new Menu { Nombre = "Reportar Averia", Path = "/reportar-averia", Orden = 4, MenuPadreId = menuMantenimiento.Id, Plataforma = Plataforma.Web, CreatedAt = DateTime.UtcNow };

        // 4. Produccion y Calidad
        var menuOrdenesFab = new Menu { Nombre = "Ordenes de Fabricacion", Path = "/ordenes-fabricacion", Orden = 1, MenuPadreId = menuProduccion.Id, Plataforma = Plataforma.Web, CreatedAt = DateTime.UtcNow };
        var menuControlCalidad = new Menu { Nombre = "Control de Calidad", Path = "/control-calidad", Orden = 2, MenuPadreId = menuProduccion.Id, Plataforma = Plataforma.Web, CreatedAt = DateTime.UtcNow };

        // 5. Gestion Comercial
        var menuClientes = new Menu { Nombre = "Clientes", Path = "/clientes", Orden = 1, MenuPadreId = menuComercial.Id, Plataforma = Plataforma.Web, CreatedAt = DateTime.UtcNow };
        var menuFacturas = new Menu { Nombre = "Facturas", Path = "/facturas", Orden = 2, MenuPadreId = menuComercial.Id, Plataforma = Plataforma.Web, CreatedAt = DateTime.UtcNow };

        context.Menus.AddRange(
            menuUsuarios, menuRoles, menuMenus,
            menuEmpleados, menuJornadas, menuIncidencias, menuValidar,
            menuMaquinaria, menuOrdenesMant, menuTareasMant, menuReportarAveria,
            menuOrdenesFab, menuControlCalidad,
            menuClientes, menuFacturas);
        await context.SaveChangesAsync();

        // ── Roles ──────────────────────────────────────────────────────────

        var rolAdmin = new Rol { Nombre = "Administrador", Descripcion = "Acceso total al sistema", CreatedAt = DateTime.UtcNow };
        var rolRRHH = new Rol { Nombre = "Responsable RRHH", Descripcion = "Gestion completa de recursos humanos", CreatedAt = DateTime.UtcNow };
        var rolEncGeneral = new Rol { Nombre = "Encargado General", Descripcion = "Supervision global de secciones", CreatedAt = DateTime.UtcNow };

        var rolesEncargado = secciones.Select(s => new Rol
        {
            Nombre = $"Encargado {s.Nombre}",
            Descripcion = $"Encargado de la seccion {s.Nombre}",
            CreatedAt = DateTime.UtcNow,
        }).ToArray();

        var rolOperarioMant = new Rol { Nombre = "Operario Mantenimiento", Descripcion = "Ejecucion de tareas de mantenimiento", CreatedAt = DateTime.UtcNow };
        var rolReportarAverias = new Rol { Nombre = "Reportar Averias", Descripcion = "Reportar averias de maquinaria", CreatedAt = DateTime.UtcNow };
        var rolJefeProduccion = new Rol { Nombre = "Jefe de Produccion", Descripcion = "Gestion de ordenes de fabricacion", CreatedAt = DateTime.UtcNow };
        var rolComercial = new Rol { Nombre = "Comercial", Descripcion = "Gestion de clientes", CreatedAt = DateTime.UtcNow };
        var rolContabilidad = new Rol { Nombre = "Contabilidad", Descripcion = "Gestion de facturacion", CreatedAt = DateTime.UtcNow };

        context.Roles.AddRange(rolAdmin, rolRRHH, rolEncGeneral);
        context.Roles.AddRange(rolesEncargado);
        context.Roles.AddRange(rolOperarioMant, rolReportarAverias, rolJefeProduccion, rolComercial, rolContabilidad);
        await context.SaveChangesAsync();

        var encCalidad = rolesEncargado.Single(r => r.Nombre == "Encargado Calidad");
        var encMantenimiento = rolesEncargado.Single(r => r.Nombre == "Encargado Mantenimiento");

        // ── MenuRol ────────────────────────────────────────────────────────

        var menusRoles = new List<MenuRol>();

        // Admin: todos los menus
        var todosMenus = new[]
        {
            menuAdmin, menuUsuarios, menuRoles, menuMenus,
            menuRRHH, menuEmpleados, menuJornadas, menuIncidencias, menuValidar,
            menuMantenimiento, menuMaquinaria, menuOrdenesMant, menuTareasMant, menuReportarAveria,
            menuProduccion, menuOrdenesFab, menuControlCalidad,
            menuComercial, menuClientes, menuFacturas,
        };
        menusRoles.AddRange(todosMenus.Select(m => new MenuRol { MenuId = m.Id, RolId = rolAdmin.Id }));

        // RRHH padre + hijos 2.1-2.3: Responsable RRHH, Encargado General, todos Encargado [Seccion]
        var menusRRHHBase = new[] { menuRRHH, menuEmpleados, menuJornadas, menuIncidencias };
        var rolesConMenuRRHH = new[] { rolRRHH, rolEncGeneral }.Concat(rolesEncargado).ToArray();
        foreach (var menu in menusRRHHBase)
            menusRoles.AddRange(rolesConMenuRRHH.Select(r => new MenuRol { MenuId = menu.Id, RolId = r.Id }));

        // 2.4 Validar Gestiones: solo Responsable RRHH
        menusRoles.Add(new MenuRol { MenuId = menuValidar.Id, RolId = rolRRHH.Id });

        // 3. Mantenimiento padre: Encargado Mantenimiento, Operario Mantenimiento, Reportar Averias
        foreach (var rol in new[] { encMantenimiento, rolOperarioMant, rolReportarAverias })
            menusRoles.Add(new MenuRol { MenuId = menuMantenimiento.Id, RolId = rol.Id });

        // 3.1 Maquinaria: Encargado Mantenimiento
        menusRoles.Add(new MenuRol { MenuId = menuMaquinaria.Id, RolId = encMantenimiento.Id });

        // 3.2 Ordenes de Mantenimiento: Encargado Mantenimiento
        menusRoles.Add(new MenuRol { MenuId = menuOrdenesMant.Id, RolId = encMantenimiento.Id });

        // 3.3 Tareas de Mantenimiento: Encargado Mantenimiento, Operario Mantenimiento
        menusRoles.Add(new MenuRol { MenuId = menuTareasMant.Id, RolId = encMantenimiento.Id });
        menusRoles.Add(new MenuRol { MenuId = menuTareasMant.Id, RolId = rolOperarioMant.Id });

        // 3.4 Reportar Averia: Reportar Averias
        menusRoles.Add(new MenuRol { MenuId = menuReportarAveria.Id, RolId = rolReportarAverias.Id });

        // 4. Produccion y Calidad padre: Encargado General, Jefe de Produccion, Encargado Calidad
        foreach (var rol in new[] { rolEncGeneral, rolJefeProduccion, encCalidad })
            menusRoles.Add(new MenuRol { MenuId = menuProduccion.Id, RolId = rol.Id });

        // 4.1 Ordenes de Fabricacion: Encargado General, Jefe de Produccion
        menusRoles.Add(new MenuRol { MenuId = menuOrdenesFab.Id, RolId = rolEncGeneral.Id });
        menusRoles.Add(new MenuRol { MenuId = menuOrdenesFab.Id, RolId = rolJefeProduccion.Id });

        // 4.2 Control de Calidad: Encargado Calidad
        menusRoles.Add(new MenuRol { MenuId = menuControlCalidad.Id, RolId = encCalidad.Id });

        // 5. Gestion Comercial padre + 5.1 Clientes: Comercial, Contabilidad
        foreach (var menu in new[] { menuComercial, menuClientes })
        {
            menusRoles.Add(new MenuRol { MenuId = menu.Id, RolId = rolComercial.Id });
            menusRoles.Add(new MenuRol { MenuId = menu.Id, RolId = rolContabilidad.Id });
        }

        // 5.2 Facturas: Contabilidad
        menusRoles.Add(new MenuRol { MenuId = menuFacturas.Id, RolId = rolContabilidad.Id });

        context.MenusRoles.AddRange(menusRoles);
        await context.SaveChangesAsync();

        // ── PermisoRolRecurso ──────────────────────────────────────────────

        var permisos = new List<PermisoRolRecurso>();

        // 1. Administrador: todos los recursos, C+E+D, Global
        permisos.AddRange(Enum.GetValues<RecursoCodigo>()
            .Select(r => Permiso(rolAdmin.Id, r, true, true, true, Alcance.Global)));

        // 2. Responsable RRHH: Empleados/Vacaciones/Turnos/Marcajes C+E+D, Global
        foreach (var r in new[] { RecursoCodigo.Empleados, RecursoCodigo.Vacaciones, RecursoCodigo.Turnos, RecursoCodigo.Marcajes })
            permisos.Add(Permiso(rolRRHH.Id, r, true, true, true, Alcance.Global));

        // 3. Encargado General: Empleados read Global, Vac/Turnos/Marcajes C Global, OrdenesFabrica read Global
        permisos.Add(Permiso(rolEncGeneral.Id, RecursoCodigo.Empleados, false, false, false, Alcance.Global));
        foreach (var r in new[] { RecursoCodigo.Vacaciones, RecursoCodigo.Turnos, RecursoCodigo.Marcajes })
            permisos.Add(Permiso(rolEncGeneral.Id, r, true, false, false, Alcance.Global));
        permisos.Add(Permiso(rolEncGeneral.Id, RecursoCodigo.OrdenesFabrica, false, false, false, Alcance.Global));

        // 4-13. Encargado [Seccion]: base + extras para Calidad y Mantenimiento
        foreach (var rol in rolesEncargado)
        {
            permisos.Add(Permiso(rol.Id, RecursoCodigo.Empleados, false, false, false, Alcance.Seccion));

            foreach (var r in new[] { RecursoCodigo.Vacaciones, RecursoCodigo.Turnos, RecursoCodigo.Marcajes })
                permisos.Add(Permiso(rol.Id, r, true, false, false, Alcance.Seccion));

            if (rol == encCalidad)
                permisos.Add(Permiso(rol.Id, RecursoCodigo.OrdenesFabrica, true, true, false, Alcance.Global));
            else
                permisos.Add(Permiso(rol.Id, RecursoCodigo.OrdenesFabrica, false, false, false, Alcance.Seccion));

            if (rol == encMantenimiento)
            {
                permisos.Add(Permiso(rol.Id, RecursoCodigo.Maquinaria, true, true, true, Alcance.Global));
                permisos.Add(Permiso(rol.Id, RecursoCodigo.Mantenimiento, true, true, true, Alcance.Global));
            }
        }

        // 14. Operario Mantenimiento: Mantenimiento C+E, Global
        permisos.Add(Permiso(rolOperarioMant.Id, RecursoCodigo.Mantenimiento, true, true, false, Alcance.Global));

        // 15. Reportar Averias: Mantenimiento C, Global
        permisos.Add(Permiso(rolReportarAverias.Id, RecursoCodigo.Mantenimiento, true, false, false, Alcance.Global));

        // 16. Jefe de Produccion: OrdenesFabrica C+E+D, Global
        permisos.Add(Permiso(rolJefeProduccion.Id, RecursoCodigo.OrdenesFabrica, true, true, true, Alcance.Global));

        // 17. Comercial: Clientes C+E+D, Global
        permisos.Add(Permiso(rolComercial.Id, RecursoCodigo.Clientes, true, true, true, Alcance.Global));

        // 18. Contabilidad: Facturas C+E+D Global, Clientes read Global
        permisos.Add(Permiso(rolContabilidad.Id, RecursoCodigo.Facturas, true, true, true, Alcance.Global));
        permisos.Add(Permiso(rolContabilidad.Id, RecursoCodigo.Clientes, false, false, false, Alcance.Global));

        context.PermisosRolRecurso.AddRange(permisos);
        await context.SaveChangesAsync();

        // ── Usuario admin ──────────────────────────────────────────────────

        var usuarioAdmin = new Usuario
        {
            Email = Email.From("admin@erpnet.com"),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Activo = true, EmpleadoId = empAdmin.Id, CreatedAt = DateTime.UtcNow,
        };
        context.Usuarios.Add(usuarioAdmin);
        await context.SaveChangesAsync();

        context.RolesUsuarios.Add(new RolUsuario { UsuarioId = usuarioAdmin.Id, RolId = rolAdmin.Id });
        await context.SaveChangesAsync();

        // ── Datos masivos ──────────────────────────────────────

        static string GenerarDni(int numero)
        {
            const string letras = "TRWAGMYFPDXBNJZSQVHLCKE";
            return $"{numero:D8}{letras[numero % 23]}";
        }

        // Empleados
        var seccionIds = secciones.Select(s => s.Id).ToArray();
        var empIndex = 0;

        var fakerEmpleado = new Faker<Empleado>("es")
            .UseSeed(42)
            .RuleFor(e => e.Nombre, f => f.Name.FirstName())
            .RuleFor(e => e.Apellidos, f => f.Name.LastName())
            .RuleFor(e => e.DNI, _ => Dni.From(GenerarDni(10000001 + empIndex)))
            .RuleFor(e => e.Activo, f => f.Random.Bool(0.95f))
            .RuleFor(e => e.SeccionId, _ => seccionIds[empIndex % seccionIds.Length])
            .RuleFor(e => e.CreatedAt, DateTime.UtcNow)
            .FinishWith((_, _) => empIndex++);

        var empleados = fakerEmpleado.Generate(350);
        context.Empleados.AddRange(empleados);
        await context.SaveChangesAsync();
        output.WriteLine($"  {empleados.Count} empleados creados.");

        // Maquinas
        var maqIndex = 0;

        var fakerMaquinaria = new Faker<Maquinaria>("es")
            .UseSeed(42)
            .RuleFor(m => m.Nombre, f => f.Commerce.ProductName())
            .RuleFor(m => m.Codigo, _ => $"MAQ-{maqIndex + 1:D4}")
            .RuleFor(m => m.Ubicacion, f => $"Nave {f.Random.Number(1, 5)}, Zona {f.Random.Char('A', 'F')}")
            .RuleFor(m => m.Activa, f => f.Random.Bool(0.9f))
            .RuleFor(m => m.SeccionId, f => f.PickRandom(seccionIds))
            .RuleFor(m => m.CreatedAt, DateTime.UtcNow)
            .FinishWith((_, _) => maqIndex++);

        var maquinarias = fakerMaquinaria.Generate(600);
        context.Maquinas.AddRange(maquinarias);
        await context.SaveChangesAsync();
        output.WriteLine($"  {maquinarias.Count} maquinarias creadas.");

        // 25 Usuarios desde los primeros 25 empleados
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Test123!");
        var usuariosNuevos = new List<Usuario>();

        for (var i = 0; i < 25; i++)
        {
            var emp = empleados[i];
            var nombre = emp.Nombre.ToLowerInvariant().Replace(" ", "");
            var apellido = emp.Apellidos.ToLowerInvariant().Replace(" ", "");
            var email = $"{nombre}.{apellido}{i + 1}@erpnet.com";

            usuariosNuevos.Add(new Usuario
            {
                Email = Email.From(email),
                PasswordHash = passwordHash,
                Activo = true,
                EmpleadoId = emp.Id,
                CreatedAt = DateTime.UtcNow,
            });
        }

        context.Usuarios.AddRange(usuariosNuevos);
        await context.SaveChangesAsync();

        // Asignacion de roles a los 25 usuarios
        var rolesUsuarios = new List<RolUsuario>();

        // Indices en usuariosNuevos:
        //  0-1:  Responsable RRHH (2)
        //  2:    Encargado General (1) + Reportar Averias
        //  3-12: Encargado [Seccion] x10 + Reportar Averias
        // 13-15: Operario Mantenimiento (3) + Reportar Averias
        // 16:    Jefe de Produccion (1) + Reportar Averias
        // 17-18: Comercial (2)
        // 19:    Contabilidad (1)
        // 20-24: Solo Reportar Averias (5)

        void AsignarRol(int idx, Rol rol, bool conReportarAverias = false)
        {
            rolesUsuarios.Add(new RolUsuario { UsuarioId = usuariosNuevos[idx].Id, RolId = rol.Id });
            if (conReportarAverias)
                rolesUsuarios.Add(new RolUsuario { UsuarioId = usuariosNuevos[idx].Id, RolId = rolReportarAverias.Id });
        }

        // 0-1: Responsable RRHH
        AsignarRol(0, rolRRHH);
        AsignarRol(1, rolRRHH);

        // 2: Encargado General
        AsignarRol(2, rolEncGeneral, conReportarAverias: true);

        // 3-12: Encargado [Seccion] x10
        for (var i = 0; i < 10; i++)
            AsignarRol(3 + i, rolesEncargado[i], conReportarAverias: true);

        // 13-15: Operario Mantenimiento
        for (var i = 13; i <= 15; i++)
            AsignarRol(i, rolOperarioMant, conReportarAverias: true);

        // 16: Jefe de Produccion
        AsignarRol(16, rolJefeProduccion, conReportarAverias: true);

        // 17-18: Comercial
        AsignarRol(17, rolComercial);
        AsignarRol(18, rolComercial);

        // 19: Contabilidad
        AsignarRol(19, rolContabilidad);

        // 20-24: Solo Reportar Averias
        for (var i = 20; i <= 24; i++)
            AsignarRol(i, rolReportarAverias);

        context.RolesUsuarios.AddRange(rolesUsuarios);
        await context.SaveChangesAsync();
        output.WriteLine($"  {usuariosNuevos.Count} usuarios creados (password: Test123!).");

        output.WriteLine("Seed completado.");
        output.WriteLine("  admin@erpnet.com / Admin123! (Administrador)");
    }
}
