using System.Security.Cryptography;
using ERPNet.Application.Auth;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.DTOs.Mappings;
using ERPNet.Application.Common.Enums;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Application.Mailing;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;

namespace ERPNet.Application.Common;

public class UsuarioService(
    IUsuarioRepository usuarioRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IMailService mailService) : IUsuarioService
{
    public async Task<Result<ListaPaginada<UsuarioResponse>>> GetAllAsync(PaginacionFilter filtro)
    {
        var (usuarios, total) = await usuarioRepository.GetPaginatedAsync(filtro);
        var response = usuarios.Select(u => u.ToResponse()).ToList();
        return Result<ListaPaginada<UsuarioResponse>>.Success(
            ListaPaginada<UsuarioResponse>.Crear(response, total, filtro));
    }

    public async Task<Result<UsuarioResponse>> GetByIdAsync(int id)
    {
        var usuario = await usuarioRepository.GetByIdAsync(id);

        if (usuario is null)
            return Result<UsuarioResponse>.Failure("Usuario no encontrado.", ErrorType.NotFound);

        return Result<UsuarioResponse>.Success(usuario.ToResponse());
    }

    public async Task<Result<UsuarioResponse>> GetMeAsync(UsuarioContext ctx)
    {
        var usuario = await usuarioRepository.GetByIdAsync(ctx.Id);

        if (usuario is null)
            return Result<UsuarioResponse>.Failure("Usuario no encontrado.", ErrorType.NotFound);

        var roles = await usuarioRepository.GetRolesConNombreAsync(ctx.Id);
        return Result<UsuarioResponse>.Success(usuario.ToResponse(roles, ctx));
    }

    public async Task<Result<UsuarioResponse>> CreateAsync(CreateUsuarioRequest request)
    {
        if (await usuarioRepository.ExisteEmailAsync(request.Email))
            return Result<UsuarioResponse>.Failure("Ya existe un usuario con ese email.", ErrorType.Conflict);

        if (await usuarioRepository.ExisteEmpleadoAsync(request.EmpleadoId))
            return Result<UsuarioResponse>.Failure("Ya existe un usuario asociado a ese empleado.", ErrorType.Conflict);

        var contrasenaTemp = GenerarContrasenaAleatoria();
        var usuario = request.ToEntity(BCrypt.Net.BCrypt.HashPassword(contrasenaTemp));
        usuario.CaducidadContrasena = DateTime.UtcNow;  // fuerza cambio en el primer acceso

        await usuarioRepository.CreateAsync(usuario);
        await unitOfWork.SaveChangesAsync();

        await mailService.EnviarBienvenidaAsync(usuario.Email, usuario.Email, contrasenaTemp);

        return Result<UsuarioResponse>.Success(usuario.ToResponse());
    }

    public async Task<Result> UpdateAsync(int id, UpdateUsuarioRequest request)
    {
        var usuario = await usuarioRepository.GetByIdAsync(id);

        if (usuario is null)
            return Result.Failure("Usuario no encontrado.", ErrorType.NotFound);

        if (request.Email is not null && request.Email != usuario.Email.Value)
        {
            if (await usuarioRepository.ExisteEmailAsync(request.Email, id))
                return Result.Failure("Ya existe un usuario con ese email.", ErrorType.Conflict);
        }

        request.ApplyTo(usuario);
        await unitOfWork.SaveChangesAsync();
        cache.Remove($"usuario:{id}");

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var usuario = await usuarioRepository.GetByIdAsync(id);

        if (usuario is null)
            return Result.Failure("Usuario no encontrado.", ErrorType.NotFound);

        usuarioRepository.Delete(usuario);
        await unitOfWork.SaveChangesAsync();
        cache.Remove($"usuario:{id}");

        return Result.Success();
    }

    public async Task<Result> CambiarContrasenaAsync(int usuarioId, CambiarContrasenaRequest request)
    {
        var usuario = await usuarioRepository.GetByIdAsync(usuarioId);

        if (usuario is null)
            return Result.Failure("Usuario no encontrado.", ErrorType.NotFound);

        if (!BCrypt.Net.BCrypt.Verify(request.ContrasenaActual, usuario.PasswordHash))
            return Result.Failure("La contraseña actual es incorrecta.", ErrorType.Validation);

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NuevaContrasena);
        usuario.UltimoCambioContrasena = DateTime.UtcNow;
        usuario.CaducidadContrasena = DateTime.UtcNow.AddDays(ContrasenaSettings.DiasExpiracionPorDefecto);
        await unitOfWork.SaveChangesAsync();
        cache.Remove($"usuario:{usuarioId}");

        return Result.Success();
    }

    public async Task<Result> ResetearContrasenaAsync(int usuarioId)
    {
        var usuario = await usuarioRepository.GetByIdAsync(usuarioId);

        if (usuario is null)
            return Result.Failure("Usuario no encontrado.", ErrorType.NotFound);

        var contrasenaTemp = GenerarContrasenaAleatoria();
        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(contrasenaTemp);
        usuario.UltimoCambioContrasena = DateTime.UtcNow;
        usuario.CaducidadContrasena = DateTime.UtcNow;  // caducada: fuerza cambio en el próximo acceso
        await unitOfWork.SaveChangesAsync();
        cache.Remove($"usuario:{usuarioId}");

        await mailService.EnviarAsync(
            usuario.Email,
            "Restablecimiento de contraseña",
            $"Un administrador ha restablecido tu contraseña.\n\nContraseña temporal: {contrasenaTemp}\n\nDeberás cambiarla en tu próximo acceso.");

        return Result.Success();
    }

    private static string GenerarContrasenaAleatoria()
    {
        const string upper = "ABCDEFGHJKMNPQRSTUVWXYZ";
        const string lower = "abcdefghjkmnpqrstuvwxyz";
        const string digits = "23456789";
        const string all = upper + lower + digits;

        var chars = new char[12];
        chars[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
        chars[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
        chars[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
        for (int i = 3; i < 12; i++)
            chars[i] = all[RandomNumberGenerator.GetInt32(all.Length)];

        RandomNumberGenerator.Shuffle(chars.AsSpan());
        return new string(chars);
    }

    public async Task<Result<List<int>>> GetRolesAsync(int usuarioId)
    {
        var usuario = await usuarioRepository.GetByIdAsync(usuarioId);

        if (usuario is null)
            return Result<List<int>>.Failure("Usuario no encontrado.", ErrorType.NotFound);

        var rolIds = await usuarioRepository.GetRolIdsAsync(usuarioId);
        return Result<List<int>>.Success(rolIds);
    }

    public async Task<Result> AsignarRolesAsync(int usuarioId, AsignarRolesRequest request)
    {
        var usuario = await usuarioRepository.GetByIdAsync(usuarioId);

        if (usuario is null)
            return Result.Failure("Usuario no encontrado.", ErrorType.NotFound);

        await usuarioRepository.SincronizarRolesAsync(usuarioId, request.RolIds);
        await unitOfWork.SaveChangesAsync();
        cache.Remove($"usuario:{usuarioId}");

        return Result.Success();
    }
}
