using ERPNet.Application.Common;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.UnitTests.Services;

public class SeccionServiceTests
{
    private readonly ISeccionRepository _repo = Substitute.For<ISeccionRepository>();
    private readonly SeccionService _sut;

    public SeccionServiceTests()
    {
        _sut = new SeccionService(_repo);
    }

    [Fact(DisplayName = "GetAll: devuelve lista de secciones ordenada")]
    public async Task GetAll_DevuelveListaSecciones()
    {
        var secciones = new List<Seccion>
        {
            new() { Id = 1, Nombre = "Almacén" },
            new() { Id = 2, Nombre = "Producción" }
        };
        _repo.GetAllAsync().Returns(secciones);

        var result = await _sut.GetAllAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Equal("Almacén", result.Value[0].Nombre);
    }

    [Fact(DisplayName = "GetAll: repositorio vacío devuelve lista vacía")]
    public async Task GetAll_RepositorioVacio_DevuelveListaVacia()
    {
        _repo.GetAllAsync().Returns([]);

        var result = await _sut.GetAllAsync();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }
}
