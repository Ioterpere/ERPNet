using ERPNet.Application.Common.DTOs;

namespace ERPNet.Application.Auth.DTOs.Mappings;

public static class UsuarioContextMappings
{
    public static AccountResponse ToResponse(this UsuarioContext ctx) => new()
    {
        Id = ctx.Id,
        Email = ctx.Email,
        EmpleadoId = ctx.EmpleadoId,
        SeccionId = ctx.SeccionId,
        Roles = ctx.RolIds,
        RequiereCambioContrasena = ctx.RequiereCambioContrasena,
    };
}
