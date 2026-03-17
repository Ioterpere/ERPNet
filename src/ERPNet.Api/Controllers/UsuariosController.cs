using ERPNet.Application.Common.Interfaces;
using ERPNet.Application.Common;
using ERPNet.Application.Common.DTOs;
using Microsoft.AspNetCore.Mvc;
using ERPNet.Api.Attributes;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;
using ERPNet.Api.Controllers.Common;

namespace ERPNet.Api.Controllers;

[Recurso(RecursoCodigo.Aplicacion)]
public class UsuariosController(IUsuarioService usuarioService, IEmpresaService empresaService) : BaseController
{
    [HttpGet]
    [ProducesResponseType<ListaPaginada<UsuarioResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginacionFilter filtro)
        => FromResult(await usuarioService.GetAllAsync(filtro));

    [HttpGet("{id}")]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
        => FromResult(await usuarioService.GetByIdAsync(id));

    [HttpPost]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateUsuarioRequest request)
        => CreatedFromResult(await usuarioService.CreateAsync(request));

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUsuarioRequest request)
        => FromResult(await usuarioService.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
        => FromResult(await usuarioService.DeleteAsync(id));

    [SinPermiso]
    [PermitirContrasenaCaducada]
    [HttpGet("account")]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe()
        => FromResult(await usuarioService.GetMeAsync(UsuarioActual));

    [SinPermiso]
    [PermitirContrasenaCaducada]
    [HttpPut("cambiar-contrasena")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CambiarContrasena([FromBody] CambiarContrasenaRequest request)
        => FromResult(await usuarioService.CambiarContrasenaAsync(UsuarioActual.Id, request));

    [HttpPut("{id}/resetear-contrasena")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetearContrasena(int id)
        => FromResult(await usuarioService.ResetearContrasenaAsync(id));

    [HttpPut("{id}/roles")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AsignarRoles(int id, [FromBody] AsignarRolesRequest request, [FromQuery] int? empresaId = null)
        => FromResult(await usuarioService.AsignarRolesAsync(id, request, empresaId));

    [HttpGet("{id}/roles/todas")]
    [ProducesResponseType<List<AsignacionRolDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodasAsignaciones(int id)
        => FromResult(await usuarioService.GetTodasAsignacionesRolAsync(id));

    [HttpPut("{id}/roles/todas")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SincronizarTodasAsignaciones(int id, [FromBody] List<AsignacionRolDto> asignaciones)
        => FromResult(await usuarioService.SincronizarTodasAsignacionesRolAsync(id, asignaciones));

    [HttpGet("{id}/empresas")]
    [ProducesResponseType<List<int>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmpresas(int id)
    {
        var result = await empresaService.GetEmpresasDeUsuarioAsync(id);
        if (!result.IsSuccess) return FromResult(result);
        return Ok(result.Value!.Select(e => e.Id).ToList());
    }

    [HttpPut("{id}/empresas")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AsignarEmpresas(int id, [FromBody] AsignarEmpresasRequest request)
        => FromResult(await empresaService.SincronizarEmpresasDeUsuarioAsync(id, request));
}
