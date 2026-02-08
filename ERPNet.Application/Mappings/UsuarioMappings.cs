using ERPNet.Application.DTOs;
using ERPNet.Domain.Entities;

namespace ERPNet.Application.Mappings;

public static class UsuarioMappings
{
    public static UsuarioResponse ToResponse(this Usuario usuario) => new()
    {
        Id = usuario.Id,
        Email = usuario.Email,
        EmpleadoId = usuario.EmpleadoId,
        Activo = usuario.Activo,
        UltimoAcceso = usuario.UltimoAcceso
    };

    public static Usuario ToEntity(this CreateUsuarioRequest request, string passwordHash) => new()
    {
        Email = request.Email,
        PasswordHash = passwordHash,
        EmpleadoId = request.EmpleadoId,
        Activo = true,
        CreatedAt = DateTime.UtcNow
    };
}
