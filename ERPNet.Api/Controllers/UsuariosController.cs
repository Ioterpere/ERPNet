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
public class UsuariosController(IUsuarioService usuarioService) : BaseController
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
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUsuarioRequest request)
        => FromResult(await usuarioService.UpdateAsync(id, request));

    [HttpDelete("{id}")]
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

    [HttpGet("{id}/roles")]
    [ProducesResponseType<List<int>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles(int id)
        => FromResult(await usuarioService.GetRolesAsync(id));

    [HttpPut("{id}/roles")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AsignarRoles(int id, [FromBody] AsignarRolesRequest request)
        => FromResult(await usuarioService.AsignarRolesAsync(id, request));
}
