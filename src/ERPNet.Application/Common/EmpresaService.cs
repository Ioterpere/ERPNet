using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.DTOs.Mappings;
using ERPNet.Application.Common.Enums;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;

namespace ERPNet.Application.Common;

public class EmpresaService(
    IEmpresaRepository empresaRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache) : IEmpresaService
{
    public async Task<Result<ListaPaginada<EmpresaResponse>>> GetAllAsync(PaginacionFilter filtro)
    {
        var (empresas, total) = await empresaRepository.GetPaginatedAsync(filtro);
        var response = empresas.Select(e => e.ToResponse()).ToList();
        return Result<ListaPaginada<EmpresaResponse>>.Success(
            ListaPaginada<EmpresaResponse>.Crear(response, total));
    }

    public async Task<Result<EmpresaResponse>> GetByIdAsync(int id)
    {
        var empresa = await empresaRepository.GetByIdAsync(id);

        if (empresa is null)
            return Result<EmpresaResponse>.Failure("Empresa no encontrada.", ErrorType.NotFound);

        return Result<EmpresaResponse>.Success(empresa.ToResponse());
    }

    public async Task<Result<EmpresaResponse>> CreateAsync(CreateEmpresaRequest request)
    {
        var empresa = request.ToEntity();
        await empresaRepository.CreateAsync(empresa);
        await unitOfWork.SaveChangesAsync();

        return Result<EmpresaResponse>.Success(empresa.ToResponse());
    }

    public async Task<Result> UpdateAsync(int id, UpdateEmpresaRequest request)
    {
        var empresa = await empresaRepository.GetByIdAsync(id);

        if (empresa is null)
            return Result.Failure("Empresa no encontrada.", ErrorType.NotFound);

        request.ApplyTo(empresa);
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var empresa = await empresaRepository.GetByIdAsync(id);

        if (empresa is null)
            return Result.Failure("Empresa no encontrada.", ErrorType.NotFound);

        empresaRepository.Delete(empresa);
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<IEnumerable<EmpresaResponse>>> GetEmpresasDeUsuarioAsync(int usuarioId)
    {
        var empresas = await empresaRepository.GetEmpresasDeUsuarioAsync(usuarioId);
        return Result<IEnumerable<EmpresaResponse>>.Success(empresas.Select(e => e.ToResponse()));
    }

    public async Task<Result> SincronizarEmpresasDeUsuarioAsync(int usuarioId, AsignarEmpresasRequest request)
    {
        await empresaRepository.SincronizarEmpresasDeUsuarioAsync(usuarioId, request.EmpresaIds);
        await unitOfWork.SaveChangesAsync();

        cache.RemoveByPrefix($"usuario:{usuarioId}:");

        return Result.Success();
    }
}
