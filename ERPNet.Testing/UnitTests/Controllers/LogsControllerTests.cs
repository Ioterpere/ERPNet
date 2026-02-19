using ERPNet.Api.Controllers;
using ERPNet.Contracts;
using ERPNet.Contracts.DTOs;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.UnitTests.Controllers;

public class LogsControllerTests
{
    private readonly ILogRepository _logRepository = Substitute.For<ILogRepository>();
    private readonly LogsController _sut;

    public LogsControllerTests()
    {
        _sut = new LogsController(_logRepository);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact(DisplayName = "GetLogs: devuelve lista paginada con datos")]
    public async Task GetLogs_DevuelveListaPaginada()
    {
        var logs = new List<Log>
        {
            new() { Id = 1, Accion = "Login", Fecha = DateTime.UtcNow },
            new() { Id = 2, Accion = "Crear", Entidad = "Usuario", EntidadId = "1", Fecha = DateTime.UtcNow }
        };
        _logRepository.GetFilteredAsync(Arg.Any<LogFilter>()).Returns((logs, 2));

        var result = await _sut.GetLogs(new LogFilter());

        var okResult = Assert.IsType<OkObjectResult>(result);
        var lista = Assert.IsType<ListaPaginada<LogResponse>>(okResult.Value);
        Assert.Equal(2, lista.TotalRegistros);
        Assert.Equal(2, lista.Items.Count);
        Assert.Equal("Login", lista.Items[0].Accion);
    }

    [Fact(DisplayName = "GetLogs: sin resultados devuelve lista vacia")]
    public async Task GetLogs_SinResultados_DevuelveListaVacia()
    {
        _logRepository.GetFilteredAsync(Arg.Any<LogFilter>()).Returns((new List<Log>(), 0));

        var result = await _sut.GetLogs(new LogFilter());

        var okResult = Assert.IsType<OkObjectResult>(result);
        var lista = Assert.IsType<ListaPaginada<LogResponse>>(okResult.Value);
        Assert.Empty(lista.Items);
        Assert.Equal(0, lista.TotalRegistros);
    }
}
