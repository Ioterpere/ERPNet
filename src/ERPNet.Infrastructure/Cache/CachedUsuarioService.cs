using ERPNet.Application.Auth.DTOs;
using ERPNet.Application.Cache;
using ERPNet.Application.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Filters;

namespace ERPNet.Infrastructure.Cache;

public sealed class CachedUsuarioService(
    IUsuarioService inner,
    ICacheService cache) : IUsuarioService
{
    private const string UsuarioPrefix = "usuario:";
    private const string MenuPrefix = "menu:";

    public async Task<Result> UpdateAsync(int id, UpdateUsuarioRequest request)
    {
        var result = await inner.UpdateAsync(id, request);
        if (result.IsSuccess) InvalidarUsuario(id);
        return result;
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var result = await inner.DeleteAsync(id);
        if (result.IsSuccess) InvalidarUsuario(id);
        return result;
    }

    public async Task<Result> CambiarContrasenaAsync(int usuarioId, CambiarContrasenaRequest request)
    {
        var result = await inner.CambiarContrasenaAsync(usuarioId, request);
        if (result.IsSuccess) InvalidarUsuario(usuarioId);
        return result;
    }

    public async Task<Result> ResetearContrasenaAsync(int usuarioId)
    {
        var result = await inner.ResetearContrasenaAsync(usuarioId);
        if (result.IsSuccess) InvalidarUsuario(usuarioId);
        return result;
    }

    public async Task<Result> AsignarRolesAsync(int usuarioId, AsignarRolesRequest request, int? empresaId = null)
    {
        var result = await inner.AsignarRolesAsync(usuarioId, request, empresaId);
        if (result.IsSuccess) InvalidarUsuario(usuarioId);
        return result;
    }

    public async Task<Result> SincronizarTodasAsignacionesRolAsync(int usuarioId, List<AsignacionRolDto> asignaciones)
    {
        var result = await inner.SincronizarTodasAsignacionesRolAsync(usuarioId, asignaciones);
        if (result.IsSuccess) InvalidarUsuario(usuarioId);
        return result;
    }

    // Delegaciones sin lógica de caché
    public Task<Result<ListaPaginada<UsuarioResponse>>> GetAllAsync(PaginacionFilter filtro)
        => inner.GetAllAsync(filtro);

    public Task<Result<UsuarioResponse>> GetByIdAsync(int id)
        => inner.GetByIdAsync(id);

    public Task<Result<UsuarioResponse>> GetMeAsync(UsuarioContext usuario)
        => inner.GetMeAsync(usuario);

    public Task<Result<UsuarioResponse>> CreateAsync(CreateUsuarioRequest request)
        => inner.CreateAsync(request);

    public Task<Result<List<AsignacionRolDto>>> GetTodasAsignacionesRolAsync(int usuarioId)
        => inner.GetTodasAsignacionesRolAsync(usuarioId);

    private void InvalidarUsuario(int usuarioId)
    {
        cache.RemoveByPrefix($"{UsuarioPrefix}{usuarioId}:");
        cache.RemoveByPrefix($"{MenuPrefix}{usuarioId}:");
    }
}
