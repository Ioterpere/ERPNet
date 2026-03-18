using ERPNet.Application.Cache;
using ERPNet.Application.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Filters;

namespace ERPNet.Infrastructure.Cache;

public sealed class CachedEmpresaService(
    IEmpresaService inner,
    ICacheService cache) : IEmpresaService
{
    public async Task<Result> SincronizarEmpresasDeUsuarioAsync(int usuarioId, AsignarEmpresasRequest request)
    {
        var result = await inner.SincronizarEmpresasDeUsuarioAsync(usuarioId, request);
        if (result.IsSuccess)
        {
            cache.RemoveByPrefix($"usuario:{usuarioId}:");
            cache.RemoveByPrefix($"menu:{usuarioId}:");
        }
        return result;
    }

    // Delegaciones sin lógica de caché
    public Task<Result<ListaPaginada<EmpresaResponse>>> GetAllAsync(PaginacionFilter filtro)
        => inner.GetAllAsync(filtro);

    public Task<Result<EmpresaResponse>> GetByIdAsync(int id)
        => inner.GetByIdAsync(id);

    public Task<Result<EmpresaResponse>> CreateAsync(CreateEmpresaRequest request)
        => inner.CreateAsync(request);

    public Task<Result> UpdateAsync(int id, UpdateEmpresaRequest request)
        => inner.UpdateAsync(id, request);

    public Task<Result> DeleteAsync(int id)
        => inner.DeleteAsync(id);

    public Task<Result<IEnumerable<EmpresaResponse>>> GetEmpresasDeUsuarioAsync(int usuarioId)
        => inner.GetEmpresasDeUsuarioAsync(usuarioId);
}
