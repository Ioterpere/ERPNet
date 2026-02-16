using ERPNet.Domain.Common.Values;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Filters;
using ERPNet.Infrastructure.Database.Repositories;
using Xunit;

namespace ERPNet.Testing.UnitTests.Repositories;

public class LogRepositoryTests : RepositoryTestBase
{
    private readonly LogRepository _sut;

    public LogRepositoryTests()
    {
        _sut = new LogRepository(Context);
        SeedBase();
    }

    private void SeedBase()
    {
        Context.Secciones.Add(new Seccion { Id = 1, Nombre = "IT" });
        Context.Empleados.AddRange(
            new Empleado { Id = 1, Nombre = "E1", Apellidos = "A1", DNI = Dni.From("00000001R"), SeccionId = 1 },
            new Empleado { Id = 2, Nombre = "E2", Apellidos = "A2", DNI = Dni.From("00000002W"), SeccionId = 1 });
        Context.Usuarios.AddRange(
            new Usuario { Id = 1, Email = Email.From("test01@mail.com"), PasswordHash = BCrypt.Net.BCrypt.HashPassword("Abc123!"), EmpleadoId = 1 },
            new Usuario { Id = 2, Email = Email.From("test02@mail.com"), PasswordHash = BCrypt.Net.BCrypt.HashPassword("Abc123!"), EmpleadoId = 2 }
        );
        Context.SaveChanges();
        Context.ChangeTracker.Clear();
    }

    private static Log CrearLog(long id, string accion, string? entidad = null, string? entidadId = null,
        int? usuarioId = null, DateTime? fecha = null) => new()
        {
            Id = id,
            Accion = accion,
            Entidad = entidad,
            EntidadId = entidadId,
            UsuarioId = usuarioId,
            Fecha = fecha ?? DateTime.UtcNow
        };

    #region GetFilteredAsync

    [Fact(DisplayName = "GetFiltered: sin filtros devuelve todos")]
    public async Task GetFiltered_SinFiltros_DevuelveTodos()
    {
        Context.Logs.AddRange(
            CrearLog(1, "Crear"),
            CrearLog(2, "Editar"),
            CrearLog(3, "Eliminar"));
        await SaveAndClearAsync();

        var (items, total) = await _sut.GetFilteredAsync(new LogFilter());

        Assert.Equal(3, total);
    }

    [Fact(DisplayName = "GetFiltered: filtra por Entidad")]
    public async Task GetFiltered_PorEntidad()
    {
        Context.Logs.AddRange(
            CrearLog(1, "Crear", entidad: "Empleado"),
            CrearLog(2, "Crear", entidad: "Maquinaria"));
        await SaveAndClearAsync();

        var (items, total) = await _sut.GetFilteredAsync(new LogFilter { Entidad = "Empleado" });

        Assert.Equal(1, total);
        Assert.Equal("Empleado", items[0].Entidad);
    }

    [Fact(DisplayName = "GetFiltered: filtra por EntidadId")]
    public async Task GetFiltered_PorEntidadId()
    {
        Context.Logs.AddRange(
            CrearLog(1, "Editar", entidad: "Empleado", entidadId: "5"),
            CrearLog(2, "Editar", entidad: "Empleado", entidadId: "10"));
        await SaveAndClearAsync();

        var (items, total) = await _sut.GetFilteredAsync(new LogFilter { EntidadId = "5" });

        Assert.Equal(1, total);
    }

    [Fact(DisplayName = "GetFiltered: filtra por UsuarioId")]
    public async Task GetFiltered_PorUsuarioId()
    {
        Context.Logs.AddRange(
            CrearLog(1, "Crear", usuarioId: 1),
            CrearLog(2, "Crear", usuarioId: 2));

        await SaveAndClearAsync();

        var (items, total) = await _sut.GetFilteredAsync(new LogFilter { UsuarioId = 1 });
        Assert.Equal(1, total);
    }

    [Fact(DisplayName = "GetFiltered: filtra por Accion")]
    public async Task GetFiltered_PorAccion()
    {
        Context.Logs.AddRange(
            CrearLog(1, "Crear"),
            CrearLog(2, "Eliminar"));
        await SaveAndClearAsync();

        var (items, total) = await _sut.GetFilteredAsync(new LogFilter { Accion = "Eliminar" });

        Assert.Equal(1, total);
        Assert.Equal("Eliminar", items[0].Accion);
    }

    [Fact(DisplayName = "GetFiltered: filtra por rango de fechas")]
    public async Task GetFiltered_PorFechas()
    {
        var ayer = DateTime.UtcNow.AddDays(-1);
        var hoy = DateTime.UtcNow;
        var manana = DateTime.UtcNow.AddDays(1);

        Context.Logs.AddRange(
            CrearLog(1, "Crear", fecha: ayer),
            CrearLog(2, "Crear", fecha: hoy),
            CrearLog(3, "Crear", fecha: manana));
        await SaveAndClearAsync();

        var (_, total) = await _sut.GetFilteredAsync(new LogFilter
        {
            Desde = hoy.AddHours(-1),
            Hasta = hoy.AddHours(1)
        });

        Assert.Equal(1, total);
    }

    [Fact(DisplayName = "GetFiltered: ordena por fecha descendente")]
    public async Task GetFiltered_OrdenaFechaDesc()
    {
        Context.Logs.AddRange(
            CrearLog(1, "Primero", fecha: DateTime.UtcNow.AddDays(-2)),
            CrearLog(2, "Ultimo", fecha: DateTime.UtcNow));
        await SaveAndClearAsync();

        var (items, _) = await _sut.GetFilteredAsync(new LogFilter());

        Assert.Equal("Ultimo", items[0].Accion);
    }

    [Fact(DisplayName = "GetFiltered: respeta paginacion")]
    public async Task GetFiltered_Paginacion()
    {
        for (int i = 1; i <= 5; i++)
            Context.Logs.Add(CrearLog(i, "Crear", fecha: DateTime.UtcNow.AddMinutes(i)));
        await SaveAndClearAsync();

        var (items, total) = await _sut.GetFilteredAsync(new LogFilter { Pagina = 1, PorPagina = 2 });

        Assert.Equal(5, total);
        Assert.Equal(2, items.Count);
    }

    #endregion
}
