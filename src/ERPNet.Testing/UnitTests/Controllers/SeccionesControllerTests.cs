using ERPNet.Api.Controllers;
using ERPNet.Application.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Enums;
using ERPNet.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace ERPNet.Testing.UnitTests.Controllers;

public class SeccionesControllerTests
{
    private readonly ISeccionService _service = Substitute.For<ISeccionService>();
    private readonly SeccionesController _sut;

    public SeccionesControllerTests()
    {
        _sut = new SeccionesController(_service);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact(DisplayName = "GetAll: exitoso devuelve 200 con lista")]
    public async Task GetAll_Exitoso_Devuelve200()
    {
        var secciones = new List<SeccionResponse>
        {
            new() { Id = 1, Nombre = "Almacén" },
            new() { Id = 2, Nombre = "Producción" }
        };
        _service.GetAllAsync()
            .Returns(Result<List<SeccionResponse>>.Success(secciones));

        var result = await _sut.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var lista = Assert.IsType<List<SeccionResponse>>(okResult.Value);
        Assert.Equal(2, lista.Count);
    }

    [Fact(DisplayName = "GetAll: error interno devuelve 500")]
    public async Task GetAll_ErrorInterno_Devuelve500()
    {
        _service.GetAllAsync()
            .Returns(Result<List<SeccionResponse>>.Failure("Error", ErrorType.InternalError));

        var result = await _sut.GetAll();

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}
