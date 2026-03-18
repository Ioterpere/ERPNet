using ERPNet.Application.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Enums;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.UnitTests.Services;

public class EmpresaServiceTests
{
    private readonly IEmpresaRepository _repo = Substitute.For<IEmpresaRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly EmpresaService _sut;

    public EmpresaServiceTests()
    {
        _sut = new EmpresaService(_repo, _uow);
    }

    private static Empresa CrearEmpresa(int id = 1, string nombre = "ERP Demo SA") => new()
    {
        Id = id, Nombre = nombre, Activo = true
    };

    [Fact(DisplayName = "GetAll: sin empresas devuelve lista vacía")]
    public async Task GetAll_SinEmpresas_DevuelveVacio()
    {
        _repo.GetPaginatedAsync(Arg.Any<PaginacionFilter>())
            .Returns(((List<Empresa>)[], 0));

        var result = await _sut.GetAllAsync(new PaginacionFilter());

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
    }

    [Fact(DisplayName = "GetAll: devuelve empresas paginadas")]
    public async Task GetAll_ConEmpresas_DevuelvePaginado()
    {
        var empresas = new List<Empresa> { CrearEmpresa(1), CrearEmpresa(2, "Otra SA") };
        _repo.GetPaginatedAsync(Arg.Any<PaginacionFilter>()).Returns((empresas, 2));

        var result = await _sut.GetAllAsync(new PaginacionFilter());

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalRegistros);
    }

    [Fact(DisplayName = "GetById: empresa existente devuelve empresa")]
    public async Task GetById_Existente_DevuelveEmpresa()
    {
        _repo.GetByIdAsync(1).Returns(CrearEmpresa());

        var result = await _sut.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal("ERP Demo SA", result.Value!.Nombre);
    }

    [Fact(DisplayName = "GetById: empresa inexistente devuelve NotFound")]
    public async Task GetById_Inexistente_DevuelveNotFound()
    {
        _repo.GetByIdAsync(99).Returns((Empresa?)null);

        var result = await _sut.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact(DisplayName = "Delete: empresa inexistente devuelve NotFound")]
    public async Task Delete_Inexistente_DevuelveNotFound()
    {
        _repo.GetByIdAsync(99).Returns((Empresa?)null);

        var result = await _sut.DeleteAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact(DisplayName = "Delete: empresa existente elimina y guarda")]
    public async Task Delete_Existente_EliminaYGuarda()
    {
        _repo.GetByIdAsync(1).Returns(CrearEmpresa());

        var result = await _sut.DeleteAsync(1);

        Assert.True(result.IsSuccess);
        _repo.Received(1).Delete(Arg.Any<Empresa>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
