using ERPNet.Application.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Application.Auth.DTOs.Mappings;
using ERPNet.Application.Auth.DTOs;
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
    public async Task<IActionResult> GetAll([FromQuery] PaginacionFilter filtro)
        => FromResult(await usuarioService.GetAllAsync(filtro));

    [HttpGet("{id}", Name = nameof(GetById))]
    public async Task<IActionResult> GetById(int id)
        => FromResult(await usuarioService.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUsuarioRequest request)
        => CreatedFromResult(
            await usuarioService.CreateAsync(request),
            nameof(GetById),
            r => new { id = r.Id });

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUsuarioRequest request)
        => FromResult(await usuarioService.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => FromResult(await usuarioService.DeleteAsync(id));

    [SinPermiso]
    [PermitirContrasenaCaducada]
    [HttpGet("account")]
    public IActionResult GetMe()
        => FromResult(Result<AccountResponse>.Success(UsuarioActual.ToResponse()));

    [SinPermiso]
    [PermitirContrasenaCaducada]
    [HttpPut("cambiar-contrasena")]
    public async Task<IActionResult> CambiarContrasena([FromBody] CambiarContrasenaRequest request)
        => FromResult(await usuarioService.CambiarContrasenaAsync(UsuarioActual.Id, request));

    [HttpPut("{id}/resetear-contrasena")]
    public async Task<IActionResult> ResetearContrasena(int id, [FromBody] ResetearContrasenaRequest request)
        => FromResult(await usuarioService.ResetearContrasenaAsync(id, request));

    [HttpGet("{id}/roles")]
    public async Task<IActionResult> GetRoles(int id)
        => FromResult(await usuarioService.GetRolesAsync(id));

    [HttpPut("{id}/roles")]
    public async Task<IActionResult> AsignarRoles(int id, [FromBody] AsignarRolesRequest request)
        => FromResult(await usuarioService.AsignarRolesAsync(id, request));
}
