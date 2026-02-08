using ERPNet.Database.Context;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace ERPNet.Testing.Seeders;

public class DbSeeder(ITestOutputHelper output)
{
    private const string ConnectionString =
        "Server=localhost\\SQLEXPRESS;Database=ERPNet;Trusted_Connection=true;TrustServerCertificate=true";

    [Fact(DisplayName = "Seed: Migrar BD y crear datos iniciales")]
    public async Task SeedDatabase()
    {
        var options = new DbContextOptionsBuilder<ERPNetDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        using var context = new ERPNetDbContext(options);

        output.WriteLine("Aplicando migraciones...");
        await context.Database.MigrateAsync();
        output.WriteLine("Migraciones aplicadas.");

        if (await context.Usuarios.AnyAsync())
        {
            output.WriteLine("Ya existen datos. Seed omitido.");
            return;
        }

        output.WriteLine("Insertando datos de prueba...");

        // Secciones
        var seccionAdmin = new Seccion { Nombre = "Administracion", CreatedAt = DateTime.UtcNow };
        var seccionProduccion = new Seccion { Nombre = "Produccion", CreatedAt = DateTime.UtcNow };
        context.Secciones.AddRange(seccionAdmin, seccionProduccion);
        await context.SaveChangesAsync();

        // Empleados
        var empAdmin = new Empleado
        {
            Nombre = "Admin", Apellidos = "Sistema", DNI = "00000000A",
            Activo = true, SeccionId = seccionAdmin.Id, CreatedAt = DateTime.UtcNow,
        };
        var empEncargado = new Empleado
        {
            Nombre = "Carlos", Apellidos = "Lopez", DNI = "11111111B",
            Activo = true, SeccionId = seccionProduccion.Id, CreatedAt = DateTime.UtcNow,
        };
        var empOperario = new Empleado
        {
            Nombre = "Maria", Apellidos = "Garcia", DNI = "22222222C",
            Activo = true, SeccionId = seccionProduccion.Id, CreatedAt = DateTime.UtcNow,
        };
        context.Empleados.AddRange(empAdmin, empEncargado, empOperario);
        await context.SaveChangesAsync();

        // Recursos
        var recursoVacaciones = new Recurso
        {
            Codigo = "vacaciones", Descripcion = "Gestion de vacaciones",
            CreatedAt = DateTime.UtcNow,
        };
        var recursoEmpleados = new Recurso
        {
            Codigo = "empleados", Descripcion = "Gestion de empleados",
            CreatedAt = DateTime.UtcNow,
        };
        var recursoTurnos = new Recurso
        {
            Codigo = "turnos", Descripcion = "Gestion de turnos",
            CreatedAt = DateTime.UtcNow,
        };
        context.Recursos.AddRange(recursoVacaciones, recursoEmpleados, recursoTurnos);
        await context.SaveChangesAsync();

        // Menus (vinculados a recursos)
        var menuVacaciones = new Menu
        {
            Nombre = "Vacaciones", Path = "/vacaciones", Orden = 1,
            Plataforma = Plataforma.Web, RecursoId = recursoVacaciones.Id,
            CreatedAt = DateTime.UtcNow,
        };
        var menuEmpleados = new Menu
        {
            Nombre = "Empleados", Path = "/empleados", Orden = 2,
            Plataforma = Plataforma.Web, RecursoId = recursoEmpleados.Id,
            CreatedAt = DateTime.UtcNow,
        };
        var menuTurnos = new Menu
        {
            Nombre = "Turnos", Path = "/turnos", Orden = 3,
            Plataforma = Plataforma.Web, RecursoId = recursoTurnos.Id,
            CreatedAt = DateTime.UtcNow,
        };
        context.Menus.AddRange(menuVacaciones, menuEmpleados, menuTurnos);
        await context.SaveChangesAsync();

        // Roles
        var rolAdmin = new Rol
        {
            Nombre = "Administrador", Descripcion = "Acceso total",
            CreatedAt = DateTime.UtcNow,
        };
        var rolEncargado = new Rol
        {
            Nombre = "Encargado", Descripcion = "Gestion de su seccion",
            CreatedAt = DateTime.UtcNow,
        };
        var rolEmpleado = new Rol
        {
            Nombre = "Empleado", Descripcion = "Acceso basico",
            CreatedAt = DateTime.UtcNow,
        };
        context.Roles.AddRange(rolAdmin, rolEncargado, rolEmpleado);
        await context.SaveChangesAsync();

        // Permisos
        context.PermisosRolRecurso.AddRange(
            // Admin: todo Global en los 3 recursos
            new PermisoRolRecurso
            {
                RolId = rolAdmin.Id, RecursoId = recursoVacaciones.Id,
                CanCreate = true, CanEdit = true, CanDelete = true, Alcance = Alcance.Global,
            },
            new PermisoRolRecurso
            {
                RolId = rolAdmin.Id, RecursoId = recursoEmpleados.Id,
                CanCreate = true, CanEdit = true, CanDelete = true, Alcance = Alcance.Global,
            },
            new PermisoRolRecurso
            {
                RolId = rolAdmin.Id, RecursoId = recursoTurnos.Id,
                CanCreate = true, CanEdit = true, CanDelete = true, Alcance = Alcance.Global,
            },
            // Encargado: Vacaciones con Create/Edit a nivel Seccion
            new PermisoRolRecurso
            {
                RolId = rolEncargado.Id, RecursoId = recursoVacaciones.Id,
                CanCreate = true, CanEdit = true, CanDelete = false, Alcance = Alcance.Seccion,
            },
            // Encargado: Turnos solo lectura a nivel Seccion
            new PermisoRolRecurso
            {
                RolId = rolEncargado.Id, RecursoId = recursoTurnos.Id,
                CanCreate = false, CanEdit = false, CanDelete = false, Alcance = Alcance.Seccion,
            },
            // Empleado: Vacaciones solo Create sobre sus propios datos
            new PermisoRolRecurso
            {
                RolId = rolEmpleado.Id, RecursoId = recursoVacaciones.Id,
                CanCreate = true, CanEdit = false, CanDelete = false, Alcance = Alcance.Propio,
            });
        await context.SaveChangesAsync();

        // Usuarios
        var usuarioAdmin = new Usuario
        {
            Email = "admin@erpnet.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Activo = true, EmpleadoId = empAdmin.Id, CreatedAt = DateTime.UtcNow,
        };
        var usuarioEncargado = new Usuario
        {
            Email = "carlos@erpnet.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Carlos123!"),
            Activo = true, EmpleadoId = empEncargado.Id, CreatedAt = DateTime.UtcNow,
        };
        var usuarioOperario = new Usuario
        {
            Email = "maria@erpnet.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Maria123!"),
            Activo = true, EmpleadoId = empOperario.Id, CreatedAt = DateTime.UtcNow,
        };
        context.Usuarios.AddRange(usuarioAdmin, usuarioEncargado, usuarioOperario);
        await context.SaveChangesAsync();

        // Asignar roles
        context.RolesUsuarios.AddRange(
            new RolUsuario { UsuarioId = usuarioAdmin.Id, RolId = rolAdmin.Id },
            // Encargado tiene rol Encargado + Empleado (para probar merge)
            new RolUsuario { UsuarioId = usuarioEncargado.Id, RolId = rolEncargado.Id },
            new RolUsuario { UsuarioId = usuarioEncargado.Id, RolId = rolEmpleado.Id },
            new RolUsuario { UsuarioId = usuarioOperario.Id, RolId = rolEmpleado.Id });
        await context.SaveChangesAsync();

        output.WriteLine("Seed completado.");
        output.WriteLine("  admin@erpnet.com / Admin123! (Administrador)");
        output.WriteLine("  carlos@erpnet.com / Carlos123! (Encargado + Empleado)");
        output.WriteLine("  maria@erpnet.com / Maria123! (Empleado)");
    }
}
