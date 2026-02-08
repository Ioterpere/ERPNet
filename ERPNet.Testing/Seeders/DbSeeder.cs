using ERPNet.Database.Context;
using ERPNet.Domain.Entities;
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

        var seccion = new Seccion
        {
            Nombre = "Administracion",
            CreatedAt = DateTime.UtcNow,
        };
        context.Secciones.Add(seccion);
        await context.SaveChangesAsync();

        var empleado = new Empleado
        {
            Nombre = "Admin",
            Apellidos = "Sistema",
            DNI = "00000000A",
            Activo = true,
            SeccionId = seccion.Id,
            CreatedAt = DateTime.UtcNow,
        };
        context.Empleados.Add(empleado);
        await context.SaveChangesAsync();

        var usuario = new Usuario
        {
            Email = "admin@erpnet.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Activo = true,
            EmpleadoId = empleado.Id,
            CreatedAt = DateTime.UtcNow,
        };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        output.WriteLine("Seed completado.");
        output.WriteLine("  Usuario: admin@erpnet.com");
        output.WriteLine("  Password: Admin123!");
    }
}
