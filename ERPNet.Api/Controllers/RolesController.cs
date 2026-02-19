using ERPNet.Api.Attributes;
using ERPNet.Api.Controllers.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ERPNet.Api.Controllers;

[Recurso(RecursoCodigo.Aplicacion)]
public class RolesController(IRolService rolService) : BaseController
{
    [HttpGet]
    [ProducesResponseType<ListaPaginada<RolResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginacionFilter filtro)
        => FromResult(await rolService.GetAllAsync(filtro));

    [HttpGet("recursos")]
    [ProducesResponseType<IEnumerable<RecursoResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRecursos()
        => FromResult(await rolService.GetAllRecursosAsync());

    [HttpGet("{id}")]
    [ProducesResponseType<RolResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
        => FromResult(await rolService.GetByIdAsync(id));

    [HttpGet("{id}/permisos")]
    [ProducesResponseType<IEnumerable<PermisoRolRecursoResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermisos(int id)
        => FromResult(await rolService.GetPermisosAsync(id));

    [HttpGet("{id}/usuarios")]
    [ProducesResponseType<IEnumerable<UsuarioResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsuarios(int id)
        => FromResult(await rolService.GetUsuariosAsync(id));

    [HttpPost]
    [ProducesResponseType<RolResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateRolRequest request)
        => CreatedFromResult(await rolService.CreateAsync(request));

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRolRequest request)
        => FromResult(await rolService.UpdateAsync(id, request));

    [HttpPut("{id}/permisos")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetPermisos(int id, [FromBody] SetPermisosRolRequest request)
        => FromResult(await rolService.SetPermisosAsync(id, request));

    [HttpPut("{id}/usuarios")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetUsuarios(int id, [FromBody] AsignarUsuariosRequest request)
        => FromResult(await rolService.SetUsuariosAsync(id, request));

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
        => FromResult(await rolService.DeleteAsync(id));
}
