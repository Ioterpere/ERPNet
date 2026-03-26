using ERPNet.Api.Attributes;
using ERPNet.Api.Controllers.Common;
using ERPNet.Application.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ERPNet.Api.Controllers;

[Recurso(RecursoCodigo.Articulos)]
public class ArticulosController(IArticuloService articuloService) : BaseController
{
    [HttpGet]
    [ProducesResponseType<ListaPaginada<ArticuloResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginacionFilter filtro)
        => FromResult(await articuloService.GetAllAsync(filtro));

    [HttpGet("{id}")]
    [ProducesResponseType<ArticuloResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
        => FromResult(await articuloService.GetByIdAsync(id));

    [HttpPost]
    [ProducesResponseType<ArticuloResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateArticuloRequest request)
        => CreatedFromResult(await articuloService.CreateAsync(request));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateArticuloRequest request)
        => FromResult(await articuloService.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => FromResult(await articuloService.DeleteAsync(id));

    [HttpGet("{id}/logs")]
    [ProducesResponseType<List<ArticuloLogResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogs(int id)
        => FromResult(await articuloService.GetLogsAsync(id));

    [HttpPost("{id}/logs")]
    [ProducesResponseType<ArticuloLogResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddLog(int id, [FromBody] CreateArticuloLogRequest request)
        => CreatedFromResult(await articuloService.AddLogAsync(id, request));

    [HttpGet("familias")]
    [ProducesResponseType<List<FamiliaArticuloResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFamilias()
        => FromResult(await articuloService.GetFamiliasAsync());

    [HttpGet("tipos-iva")]
    [ProducesResponseType<List<TipoIvaResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTiposIva()
        => FromResult(await articuloService.GetTiposIvaAsync());

    [HttpGet("formatos")]
    [ProducesResponseType<List<FormatoArticuloResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFormatos()
        => FromResult(await articuloService.GetFormatosAsync());

    [HttpGet("configuraciones-caducidad")]
    [ProducesResponseType<List<ConfiguracionCaducidadResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConfiguracionesCaducidad()
        => FromResult(await articuloService.GetConfiguracionesCaducidadAsync());
}
