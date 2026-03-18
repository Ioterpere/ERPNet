using ERPNet.Application.Auth.DTOs;
using ERPNet.Application.Auth.Interfaces;
using ERPNet.Application.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Enums;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.UnitTests.Services;

public class ArticuloServiceTests
{
    private readonly IArticuloRepository _repo = Substitute.For<IArticuloRepository>();
    private readonly IFamiliaArticuloRepository _familiaRepo = Substitute.For<IFamiliaArticuloRepository>();
    private readonly ICatalogoArticulosRepository _catalogoRepo = Substitute.For<ICatalogoArticulosRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserProvider _currentUser = Substitute.For<ICurrentUserProvider>();
    private readonly ArticuloService _sut;

    public ArticuloServiceTests()
    {
        _sut = new ArticuloService(_repo, _familiaRepo, _catalogoRepo, _uow, _currentUser);
    }

    private void SetupUsuario(int empresaId = 1)
    {
        var permisos = new List<PermisoResponse>
        {
            new PermisoResponse { Codigo = RecursoCodigo.Articulos, CanCreate = true, CanEdit = true, CanDelete = true, Alcance = Alcance.Global }
        };
        _currentUser.Current.Returns(new UsuarioContext
        {
            Id = 1, Email = "test@test.com", EmpresaId = empresaId,
            Permisos = permisos, RolIds = [1]
        });
    }

    private static Articulo CrearArticulo(int id = 1, int empresaId = 1) => new()
    {
        Id = id,
        Codigo = "ART-001",
        Descripcion = "Artículo de prueba",
        Activo = true,
        EmpresaId = empresaId
    };

    #region GetAllAsync

    [Fact(DisplayName = "GetAll: devuelve lista paginada de la empresa")]
    public async Task GetAll_DevuelveListaPaginada()
    {
        SetupUsuario(empresaId: 1);
        var articulos = new List<Articulo> { CrearArticulo(1), CrearArticulo(2) };
        _repo.GetPaginatedAsync(Arg.Any<PaginacionFilter>(), 1).Returns((articulos, 2));

        var result = await _sut.GetAllAsync(new PaginacionFilter());

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalRegistros);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetById: existente con acceso devuelve OK")]
    public async Task GetById_ConAcceso_DevuelveOk()
    {
        SetupUsuario(empresaId: 1);
        _repo.GetByIdAsync(1).Returns(CrearArticulo(1, empresaId: 1));

        var result = await _sut.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal("ART-001", result.Value!.Codigo);
    }

    [Fact(DisplayName = "GetById: inexistente devuelve NotFound")]
    public async Task GetById_Inexistente_DevuelveNotFound()
    {
        SetupUsuario();
        _repo.GetByIdAsync(99).Returns((Articulo?)null);

        var result = await _sut.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact(DisplayName = "GetById: empresa diferente devuelve Forbidden")]
    public async Task GetById_EmpresaDiferente_DevuelveForbidden()
    {
        SetupUsuario(empresaId: 1);
        _repo.GetByIdAsync(1).Returns(CrearArticulo(1, empresaId: 2));

        var result = await _sut.GetByIdAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "Create: datos válidos crea artículo")]
    public async Task Create_DatosValidos_CreaArticulo()
    {
        SetupUsuario(empresaId: 1);
        _repo.ExisteCodigoAsync("ART-001", 1).Returns(false);

        var result = await _sut.CreateAsync(new CreateArticuloRequest { Codigo = "ART-001", Descripcion = "Nuevo" });

        Assert.True(result.IsSuccess);
        Assert.Equal("ART-001", result.Value!.Codigo);
        await _repo.Received(1).CreateAsync(Arg.Any<Articulo>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Create: código duplicado devuelve Conflict")]
    public async Task Create_CodigoDuplicado_DevuelveConflict()
    {
        SetupUsuario(empresaId: 1);
        _repo.ExisteCodigoAsync("ART-001", 1).Returns(true);

        var result = await _sut.CreateAsync(new CreateArticuloRequest { Codigo = "ART-001", Descripcion = "X" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = "Update: datos válidos actualiza artículo")]
    public async Task Update_DatosValidos_ActualizaArticulo()
    {
        SetupUsuario(empresaId: 1);
        var articulo = CrearArticulo();
        _repo.GetByIdAsync(1).Returns(articulo);

        var result = await _sut.UpdateAsync(1, new UpdateArticuloRequest { Descripcion = "Descripción nueva" });

        Assert.True(result.IsSuccess);
        Assert.Equal("Descripción nueva", articulo.Descripcion);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Update: inexistente devuelve NotFound")]
    public async Task Update_Inexistente_DevuelveNotFound()
    {
        SetupUsuario();
        _repo.GetByIdAsync(99).Returns((Articulo?)null);

        var result = await _sut.UpdateAsync(99, new UpdateArticuloRequest { Descripcion = "X" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact(DisplayName = "Update: empresa diferente devuelve Forbidden")]
    public async Task Update_EmpresaDiferente_DevuelveForbidden()
    {
        SetupUsuario(empresaId: 1);
        _repo.GetByIdAsync(1).Returns(CrearArticulo(1, empresaId: 2));

        var result = await _sut.UpdateAsync(1, new UpdateArticuloRequest { Descripcion = "X" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
    }

    [Fact(DisplayName = "Update: código duplicado devuelve Conflict")]
    public async Task Update_CodigoDuplicado_DevuelveConflict()
    {
        SetupUsuario(empresaId: 1);
        _repo.GetByIdAsync(1).Returns(CrearArticulo());
        _repo.ExisteCodigoAsync("ART-999", 1, 1).Returns(true);

        var result = await _sut.UpdateAsync(1, new UpdateArticuloRequest { Codigo = "ART-999" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = "Delete: con acceso elimina artículo")]
    public async Task Delete_ConAcceso_Elimina()
    {
        SetupUsuario(empresaId: 1);
        var articulo = CrearArticulo();
        _repo.GetByIdAsync(1).Returns(articulo);

        var result = await _sut.DeleteAsync(1);

        Assert.True(result.IsSuccess);
        _repo.Received(1).Delete(articulo);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Delete: empresa diferente devuelve Forbidden")]
    public async Task Delete_EmpresaDiferente_DevuelveForbidden()
    {
        SetupUsuario(empresaId: 1);
        _repo.GetByIdAsync(1).Returns(CrearArticulo(1, empresaId: 2));

        var result = await _sut.DeleteAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
    }

    [Fact(DisplayName = "Delete: inexistente devuelve NotFound")]
    public async Task Delete_Inexistente_DevuelveNotFound()
    {
        SetupUsuario();
        _repo.GetByIdAsync(99).Returns((Articulo?)null);

        var result = await _sut.DeleteAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    #endregion

    #region GetLogsAsync

    [Fact(DisplayName = "GetLogs: con acceso devuelve lista de logs")]
    public async Task GetLogs_ConAcceso_DevuelveLogs()
    {
        SetupUsuario(empresaId: 1);
        _repo.GetByIdAsync(1).Returns(CrearArticulo());
        _repo.GetLogsAsync(1).Returns(new List<ArticuloLog>
        {
            new() { Id = 1, ArticuloId = 1, UsuarioId = 1, Fecha = DateOnly.FromDateTime(DateTime.Today), Nota = "Test", CreatedAt = DateTime.UtcNow }
        });

        var result = await _sut.GetLogsAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    [Fact(DisplayName = "GetLogs: artículo inexistente devuelve NotFound")]
    public async Task GetLogs_Inexistente_DevuelveNotFound()
    {
        SetupUsuario();
        _repo.GetByIdAsync(99).Returns((Articulo?)null);

        var result = await _sut.GetLogsAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact(DisplayName = "GetLogs: empresa diferente devuelve Forbidden")]
    public async Task GetLogs_EmpresaDiferente_DevuelveForbidden()
    {
        SetupUsuario(empresaId: 1);
        _repo.GetByIdAsync(1).Returns(CrearArticulo(1, empresaId: 2));

        var result = await _sut.GetLogsAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
    }

    #endregion

    #region AddLogAsync

    [Fact(DisplayName = "AddLog: con acceso añade log")]
    public async Task AddLog_ConAcceso_AñadeLog()
    {
        SetupUsuario(empresaId: 1);
        _repo.GetByIdAsync(1).Returns(CrearArticulo());

        var result = await _sut.AddLogAsync(1, new CreateArticuloLogRequest
        {
            Fecha = DateOnly.FromDateTime(DateTime.Today),
            Nota = "Entrada de mercancía"
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("Entrada de mercancía", result.Value!.Nota);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "AddLog: artículo inexistente devuelve NotFound")]
    public async Task AddLog_Inexistente_DevuelveNotFound()
    {
        SetupUsuario();
        _repo.GetByIdAsync(99).Returns((Articulo?)null);

        var result = await _sut.AddLogAsync(99, new CreateArticuloLogRequest
        {
            Fecha = DateOnly.FromDateTime(DateTime.Today),
            Nota = "Test"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact(DisplayName = "AddLog: empresa diferente devuelve Forbidden")]
    public async Task AddLog_EmpresaDiferente_DevuelveForbidden()
    {
        SetupUsuario(empresaId: 1);
        _repo.GetByIdAsync(1).Returns(CrearArticulo(1, empresaId: 2));

        var result = await _sut.AddLogAsync(1, new CreateArticuloLogRequest
        {
            Fecha = DateOnly.FromDateTime(DateTime.Today),
            Nota = "Test"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
    }

    #endregion

    #region Catálogos

    [Fact(DisplayName = "GetFamilias: devuelve familias de la empresa")]
    public async Task GetFamilias_DevuelveFamilias()
    {
        SetupUsuario(empresaId: 1);
        _familiaRepo.GetAllByEmpresaAsync(1).Returns(new List<FamiliaArticulo>
        {
            new() { Id = 1, Nombre = "Cárnicos", EmpresaId = 1 },
            new() { Id = 2, Nombre = "Lácteos", EmpresaId = 1 }
        });

        var result = await _sut.GetFamiliasAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact(DisplayName = "GetTiposIva: devuelve lista de tipos IVA")]
    public async Task GetTiposIva_DevuelveLista()
    {
        SetupUsuario();
        _catalogoRepo.GetTiposIvaAsync().Returns(new List<TipoIva>
        {
            new() { Id = 1, Nombre = "IVA 10%", Porcentaje = 10m },
            new() { Id = 2, Nombre = "IVA 21%", Porcentaje = 21m }
        });

        var result = await _sut.GetTiposIvaAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact(DisplayName = "GetFormatos: devuelve lista de formatos")]
    public async Task GetFormatos_DevuelveLista()
    {
        SetupUsuario();
        _catalogoRepo.GetFormatosAsync().Returns(new List<FormatoArticulo>
        {
            new() { Id = 1, Nombre = "Unidad" },
            new() { Id = 2, Nombre = "Caja" },
            new() { Id = 3, Nombre = "Kilogramo" }
        });

        var result = await _sut.GetFormatosAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Count);
    }

    #endregion
}
