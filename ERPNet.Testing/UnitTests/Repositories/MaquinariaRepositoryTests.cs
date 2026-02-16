using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;
using ERPNet.Infrastructure.Database.Repositories;
using Xunit;

namespace ERPNet.Testing.UnitTests.Repositories;

public class MaquinariaRepositoryTests : RepositoryTestBase
{
    private readonly MaquinariaRepository _sut;

    public MaquinariaRepositoryTests()
    {
        _sut = new MaquinariaRepository(Context);
        SeedSecciones();
    }

    private void SeedSecciones()
    {
        Context.Secciones.AddRange(
            new Seccion { Id = 1, Nombre = "Producción" },
            new Seccion { Id = 2, Nombre = "Logística" });
        Context.SaveChanges();
        Context.ChangeTracker.Clear();
    }

    private static Maquinaria Crear(int id, string codigo, int? seccionId = 1, bool deleted = false) => new()
    {
        Id = id,
        Nombre = $"Maquina{id}",
        Codigo = codigo,
        Activa = true,
        SeccionId = seccionId,
        IsDeleted = deleted
    };

    #region ExisteCodigoAsync

    [Fact(DisplayName = "ExisteCodigo: codigo existente devuelve true")]
    public async Task ExisteCodigo_Existente_True()
    {
        Context.Maquinas.Add(Crear(1, "MAQ-001"));
        await SaveAndClearAsync();

        Assert.True(await _sut.ExisteCodigoAsync("MAQ-001"));
    }

    [Fact(DisplayName = "ExisteCodigo: codigo inexistente devuelve false")]
    public async Task ExisteCodigo_Inexistente_False()
    {
        Assert.False(await _sut.ExisteCodigoAsync("MAQ-999"));
    }

    [Fact(DisplayName = "ExisteCodigo: excluye id propio")]
    public async Task ExisteCodigo_ExcluyeIdPropio()
    {
        Context.Maquinas.Add(Crear(1, "MAQ-001"));
        await SaveAndClearAsync();

        Assert.False(await _sut.ExisteCodigoAsync("MAQ-001", excludeId: 1));
    }

    [Fact(DisplayName = "ExisteCodigo: no excluye otros ids")]
    public async Task ExisteCodigo_NoExcluyeOtros()
    {
        Context.Maquinas.Add(Crear(1, "MAQ-001"));
        await SaveAndClearAsync();

        Assert.True(await _sut.ExisteCodigoAsync("MAQ-001", excludeId: 99));
    }

    #endregion

    #region GetPaginatedAsync (Alcance)

    [Fact(DisplayName = "GetPaginated: Global devuelve todas")]
    public async Task GetPaginated_Global_DevuelveTodas()
    {
        Context.Maquinas.AddRange(Crear(1, "MAQ-001", 1), Crear(2, "MAQ-002", 2));
        await SaveAndClearAsync();

        var (items, total) = await _sut.GetPaginatedAsync(new PaginacionFilter(), Alcance.Global, 1);

        Assert.Equal(2, total);
    }

    [Fact(DisplayName = "GetPaginated: Seccion filtra por seccionId")]
    public async Task GetPaginated_Seccion_Filtra()
    {
        Context.Maquinas.AddRange(Crear(1, "MAQ-001", 1), Crear(2, "MAQ-002", 2));
        await SaveAndClearAsync();

        var (items, total) = await _sut.GetPaginatedAsync(new PaginacionFilter(), Alcance.Seccion, 1);

        Assert.Equal(1, total);
        Assert.All(items, m => Assert.Equal(1, m.SeccionId));
    }

    [Fact(DisplayName = "GetPaginated: Propio devuelve vacio")]
    public async Task GetPaginated_Propio_Vacio()
    {
        Context.Maquinas.Add(Crear(1, "MAQ-001"));
        await SaveAndClearAsync();

        var (items, total) = await _sut.GetPaginatedAsync(new PaginacionFilter(), Alcance.Propio, 1);

        Assert.Equal(0, total);
        Assert.Empty(items);
    }

    [Fact(DisplayName = "GetPaginated: excluye soft-deleted")]
    public async Task GetPaginated_ExcluyeSoftDeleted()
    {
        Context.Maquinas.AddRange(Crear(1, "MAQ-001"), Crear(2, "MAQ-002", deleted: true));
        await SaveAndClearAsync();

        var (_, total) = await _sut.GetPaginatedAsync(new PaginacionFilter(), Alcance.Global, 1);

        Assert.Equal(1, total);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetById: existente devuelve maquinaria")]
    public async Task GetById_Existente_Devuelve()
    {
        Context.Maquinas.Add(Crear(1, "MAQ-001"));
        await SaveAndClearAsync();

        var result = await _sut.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("MAQ-001", result.Codigo);
    }

    [Fact(DisplayName = "GetById: soft-deleted devuelve null")]
    public async Task GetById_SoftDeleted_Null()
    {
        Context.Maquinas.Add(Crear(1, "MAQ-001", deleted: true));
        await SaveAndClearAsync();

        Assert.Null(await _sut.GetByIdAsync(1));
    }

    #endregion
}
