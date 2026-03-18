using ERPNet.Application.Cache;
using ERPNet.Application.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Filters;

namespace ERPNet.Infrastructure.Cache;

public sealed class CachedRolService(
    IRolService inner,
    ICacheService cache) : IRolService
{
    public async Task<Result> UpdateAsync(int id, UpdateRolRequest request)
    {
        var result = await inner.UpdateAsync(id, request);
        if (result.IsSuccess) InvalidarTodo();
        return result;
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var result = await inner.DeleteAsync(id);
        if (result.IsSuccess) InvalidarTodo();
        return result;
    }

    public async Task<Result> SetPermisosAsync(int rolId, SetPermisosRolRequest request)
    {
        var result = await inner.SetPermisosAsync(rolId, request);
        if (result.IsSuccess) InvalidarTodo();
        return result;
    }

    public async Task<Result> SincronizarTodasAsignacionesUsuarioAsync(int rolId, List<AsignacionUsuarioDto> asignaciones)
    {
        var result = await inner.SincronizarTodasAsignacionesUsuarioAsync(rolId, asignaciones);
        if (result.IsSuccess) InvalidarTodo();
        return result;
    }

    // Delegaciones sin lógica de caché
    public Task<Result<ListaPaginada<RolResponse>>> GetAllAsync(PaginacionFilter filtro)
        => inner.GetAllAsync(filtro);

    public Task<Result<RolResponse>> GetByIdAsync(int id)
        => inner.GetByIdAsync(id);

    public Task<Result<RolResponse>> CreateAsync(CreateRolRequest request)
        => inner.CreateAsync(request);

    public Task<Result<IEnumerable<RecursoResponse>>> GetAllRecursosAsync()
        => inner.GetAllRecursosAsync();

    public Task<Result<IEnumerable<PermisoRolRecursoResponse>>> GetPermisosAsync(int rolId)
        => inner.GetPermisosAsync(rolId);

    public Task<Result<List<AsignacionUsuarioDto>>> GetTodasAsignacionesUsuarioAsync(int rolId)
        => inner.GetTodasAsignacionesUsuarioAsync(rolId);

    private void InvalidarTodo()
    {
        cache.RemoveByPrefix("usuario:");
        cache.RemoveByPrefix("menu:");
    }
}
