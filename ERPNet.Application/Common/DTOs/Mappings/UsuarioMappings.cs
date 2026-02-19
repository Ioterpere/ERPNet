using ERPNet.Application.Auth;
using ERPNet.Contracts.DTOs;
using ERPNet.Domain.Common.Values;
using ERPNet.Domain.Entities;

namespace ERPNet.Application.Common.DTOs.Mappings;

public static class UsuarioMappings
{
    public static UsuarioResponse ToResponse(this Usuario usuario) => new()
    {
        Id = usuario.Id,
        Email = usuario.Email.Value,
        EmpleadoId = usuario.EmpleadoId,
        Activo = usuario.Activo,
        UltimoAcceso = usuario.UltimoAcceso,
        CaducidadContrasena = usuario.CaducidadContrasena,
        UltimoCambioContrasena = usuario.UltimoCambioContrasena
    };

    public static Usuario ToEntity(this CreateUsuarioRequest request, string passwordHash) => new()
    {
        Email = Email.From(request.Email),
        PasswordHash = passwordHash,
        EmpleadoId = request.EmpleadoId,
        Activo = true,
        UltimoCambioContrasena = DateTime.UtcNow,
        CaducidadContrasena = DateTime.UtcNow.AddDays(ContrasenaSettings.DiasExpiracionPorDefecto)
    };

    public static void ApplyTo(this UpdateUsuarioRequest request, Usuario usuario)
    {
        if (request.Email is not null)
            usuario.Email = Email.From(request.Email);

        if (request.EmpleadoId.HasValue)
            usuario.EmpleadoId = request.EmpleadoId.Value;

        if (request.Activo.HasValue)
            usuario.Activo = request.Activo.Value;

        if (request.CaducidadContrasena.HasValue)
            usuario.CaducidadContrasena = request.CaducidadContrasena.Value;
    }
}
