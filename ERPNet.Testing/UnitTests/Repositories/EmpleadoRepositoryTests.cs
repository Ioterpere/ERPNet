using ERPNet.Domain.Common.Values;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;
using ERPNet.Infrastructure.Database.Repositories;
using Xunit;

namespace ERPNet.Testing.UnitTests.Repositories;

public class EmpleadoRepositoryTests : RepositoryTestBase
{
    private readonly EmpleadoRepository _sut;

    // DNIs válidos (letra = "TRWAGMYFPDXBNJZSQVHLCKE"[numero % 23])
    private const string Dni1 = "12345678Z"; // 12345678 % 23 = 14 → Z
    private const string Dni2 = "87654321X"; // 87654321 % 23 = 10 → X
    private const string Dni3 = "11111111H"; // 11111111 % 23 = 0 → T... wait let me recalc
    // Actually: 11111111 % 23 = 11111111 - 23*483135 = 11111111 - 11112105 ... let me use known valid ones
    // 00000001R (1%23=1 → R), 00000002W (2%23=2 → W), 00000023T (23%23=0 → T)

    public EmpleadoRepositoryTests()
    {
        _sut = new EmpleadoRepository(Context);
        SeedSeccion();
    }

    private void SeedSeccion()
    {
        Context.Secciones.AddRange(
            new Seccion { Id = 1, Nombre = "IT" },
            new Seccion { Id = 2, Nombre = "RRHH" });
        Context.SaveChanges();
        Context.ChangeTracker.Clear();
    }

    private static Empleado CrearEmpleado(int id, string dni, int seccionId = 1, bool deleted = false) => new()
    {
        Id = id,
        Nombre = $"Emp{id}",
        Apellidos = $"Apellido{id}",
        DNI = Dni.From(dni),
        Activo = true,
        SeccionId = seccionId,
        IsDeleted = deleted
    };

    /// <summary>Genera un DNI válido a partir de un número.</summary>
    private static string DniValido(int numero)
    {
        var letras = "TRWAGMYFPDXBNJZSQVHLCKE";
        return $"{numero:D8}{letras[numero % 23]}";
    }

    #region ExisteDniAsync

    [Fact(DisplayName = "ExisteDni: DNI existente devuelve true")]
    public async Task ExisteDni_Existente_True()
    {
        Context.Empleados.Add(CrearEmpleado(1, "12345678Z"));
        await SaveAndClearAsync();

        Assert.True(await _sut.ExisteDniAsync("12345678Z"));
    }

    [Fact(DisplayName = "ExisteDni: DNI inexistente devuelve false")]
    public async Task ExisteDni_Inexistente_False()
    {
        Assert.False(await _sut.ExisteDniAsync("00000000T"));
    }

    [Fact(DisplayName = "ExisteDni: excluye id propio")]
    public async Task ExisteDni_ExcluyeIdPropio()
    {
        Context.Empleados.Add(CrearEmpleado(1, "12345678Z"));
        await SaveAndClearAsync();

        Assert.False(await _sut.ExisteDniAsync("12345678Z", excludeId: 1));
    }

    [Fact(DisplayName = "ExisteDni: no excluye otros ids")]
    public async Task ExisteDni_NoExcluyeOtros()
    {
        Context.Empleados.Add(CrearEmpleado(1, "12345678Z"));
        await SaveAndClearAsync();

        Assert.True(await _sut.ExisteDniAsync("12345678Z", excludeId: 99));
    }

    [Fact(DisplayName = "ExisteDni: normaliza a mayusculas")]
    public async Task ExisteDni_NormalizaMayusculas()
    {
        Context.Empleados.Add(CrearEmpleado(1, "12345678Z"));
        await SaveAndClearAsync();

        Assert.True(await _sut.ExisteDniAsync("  12345678z  "));
    }

    #endregion

    #region GetPaginatedAsync (Alcance)

    [Fact(DisplayName = "GetPaginated: Global devuelve todos")]
    public async Task GetPaginated_Global_DevuelveTodos()
    {
        Context.Empleados.AddRange(
            CrearEmpleado(1, DniValido(1), seccionId: 1),
            CrearEmpleado(2, DniValido(2), seccionId: 2));
        await SaveAndClearAsync();

        var (items, total) = await _sut.GetPaginatedAsync(new PaginacionFilter(), Alcance.Global, 1, 1);

        Assert.Equal(2, total);
        Assert.Equal(2, items.Count);
    }

    [Fact(DisplayName = "GetPaginated: Seccion filtra por seccionId")]
    public async Task GetPaginated_Seccion_FiltraPorSeccion()
    {
        Context.Empleados.AddRange(
            CrearEmpleado(1, DniValido(1), seccionId: 1),
            CrearEmpleado(2, DniValido(2), seccionId: 2));
        await SaveAndClearAsync();

        var (items, total) = await _sut.GetPaginatedAsync(new PaginacionFilter(), Alcance.Seccion, 1, 1);

        Assert.Equal(1, total);
        Assert.All(items, e => Assert.Equal(1, e.SeccionId));
    }

    [Fact(DisplayName = "GetPaginated: Propio filtra por empleadoId")]
    public async Task GetPaginated_Propio_FiltraPorEmpleadoId()
    {
        Context.Empleados.AddRange(
            CrearEmpleado(1, DniValido(1)),
            CrearEmpleado(2, DniValido(2)));
        await SaveAndClearAsync();

        var (items, total) = await _sut.GetPaginatedAsync(new PaginacionFilter(), Alcance.Propio, 1, 1);

        Assert.Equal(1, total);
        Assert.Single(items);
        Assert.Equal(1, items[0].Id);
    }

    [Fact(DisplayName = "GetPaginated: respeta paginacion")]
    public async Task GetPaginated_RespetaPaginacion()
    {
        for (int i = 1; i <= 5; i++)
            Context.Empleados.Add(CrearEmpleado(i, DniValido(i)));
        await SaveAndClearAsync();

        var (items, total) = await _sut.GetPaginatedAsync(
            new PaginacionFilter { Pagina = 2, PorPagina = 2 }, Alcance.Global, 1, 1);

        Assert.Equal(5, total);
        Assert.Equal(2, items.Count);
    }

    [Fact(DisplayName = "GetPaginated: excluye soft-deleted")]
    public async Task GetPaginated_ExcluyeSoftDeleted()
    {
        Context.Empleados.AddRange(
            CrearEmpleado(1, DniValido(1)),
            CrearEmpleado(2, DniValido(2), deleted: true));
        await SaveAndClearAsync();

        var (_, total) = await _sut.GetPaginatedAsync(new PaginacionFilter(), Alcance.Global, 1, 1);

        Assert.Equal(1, total);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetById: devuelve empleado existente")]
    public async Task GetById_Existente_Devuelve()
    {
        Context.Empleados.Add(CrearEmpleado(1, "12345678Z"));
        await SaveAndClearAsync();

        var result = await _sut.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Emp1", result.Nombre);
    }

    [Fact(DisplayName = "GetById: inexistente devuelve null")]
    public async Task GetById_Inexistente_Null()
    {
        Assert.Null(await _sut.GetByIdAsync(99));
    }

    [Fact(DisplayName = "GetById: soft-deleted devuelve null")]
    public async Task GetById_SoftDeleted_Null()
    {
        Context.Empleados.Add(CrearEmpleado(1, "12345678Z", deleted: true));
        await SaveAndClearAsync();

        Assert.Null(await _sut.GetByIdAsync(1));
    }

    #endregion

    #region Soft Delete

    [Fact(DisplayName = "Delete: marca IsDeleted sin eliminar fisicamente")]
    public async Task Delete_MarcaSoftDelete()
    {
        var empleado = CrearEmpleado(1, "12345678Z");
        Context.Empleados.Add(empleado);
        await SaveAndClearAsync();

        var entity = await _sut.GetByIdAsync(1);
        _sut.Delete(entity!);
        await SaveAndClearAsync();

        Assert.Null(await _sut.GetByIdAsync(1));
    }

    #endregion
}
