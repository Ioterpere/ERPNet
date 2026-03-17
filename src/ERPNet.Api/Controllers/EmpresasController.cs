using ERPNet.Api.Attributes;
using ERPNet.Api.Controllers.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ERPNet.Api.Controllers;

[Recurso(RecursoCodigo.Empresas)]
public class EmpresasController(IEmpresaService empresaService) : BaseController
{
    [HttpGet]
    [ProducesResponseType<ListaPaginada<EmpresaResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginacionFilter filtro)
        => FromResult(await empresaService.GetAllAsync(filtro));

    [HttpGet("{id}")]
    [ProducesResponseType<EmpresaResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
        => FromResult(await empresaService.GetByIdAsync(id));

    [HttpGet("mis-empresas")]
    [SinPermiso]
    [ProducesResponseType<IEnumerable<EmpresaResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMisEmpresas()
        => FromResult(await empresaService.GetEmpresasDeUsuarioAsync(UsuarioActual.Id));

    [HttpPost]
    [ProducesResponseType<EmpresaResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateEmpresaRequest request)
        => CreatedFromResult(await empresaService.CreateAsync(request));

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmpresaRequest request)
        => FromResult(await empresaService.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
        => FromResult(await empresaService.DeleteAsync(id));
}
