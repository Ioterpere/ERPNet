using ERPNet.Application.Auth.Interfaces;
using ERPNet.Application.Common.DTOs.Mappings;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Contracts;
using ERPNet.Contracts.DTOs;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;

namespace ERPNet.Application.Common;

public class MaquinariaService(
    IMaquinariaRepository maquinariaRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserProvider currentUser) : IMaquinariaService
{
    public async Task<Result<ListaPaginada<MaquinariaResponse>>> GetAllAsync(PaginacionFilter filtro)
    {
        var alcance = ObtenerAlcance();
        var seccionId = currentUser.Current!.SeccionId;

        var (maquinas, total) = await maquinariaRepository.GetPaginatedAsync(filtro, alcance, seccionId);

        var response = maquinas.Select(m => m.ToResponse()).ToList();
        return Result<ListaPaginada<MaquinariaResponse>>.Success(
            ListaPaginada<MaquinariaResponse>.Crear(response, total, filtro.Pagina, filtro.PorPagina));
    }

    public async Task<Result<MaquinariaResponse>> GetByIdAsync(int id)
    {
        var maquinaria = await maquinariaRepository.GetByIdAsync(id);

        if (maquinaria is null)
            return Result<MaquinariaResponse>.Failure("Máquina no encontrada.", ErrorType.NotFound);

        if (!TieneAcceso(maquinaria))
            return Result<MaquinariaResponse>.Failure("No tiene acceso a esta máquina.", ErrorType.Forbidden);

        return Result<MaquinariaResponse>.Success(maquinaria.ToResponse());
    }

    public async Task<Result<MaquinariaResponse>> CreateAsync(CreateMaquinariaRequest request)
    {
        if (await maquinariaRepository.ExisteCodigoAsync(request.Codigo))
            return Result<MaquinariaResponse>.Failure("Ya existe una máquina con ese código.", ErrorType.Conflict);

        var maquinaria = request.ToEntity();

        await maquinariaRepository.CreateAsync(maquinaria);
        await unitOfWork.SaveChangesAsync();

        return Result<MaquinariaResponse>.Success(maquinaria.ToResponse());
    }

    public async Task<Result> UpdateAsync(int id, UpdateMaquinariaRequest request)
    {
        var maquinaria = await maquinariaRepository.GetByIdAsync(id);

        if (maquinaria is null)
            return Result.Failure("Máquina no encontrada.", ErrorType.NotFound);

        if (!TieneAcceso(maquinaria))
            return Result.Failure("No tiene acceso a esta máquina.", ErrorType.Forbidden);

        if (request.Codigo is not null && request.Codigo != maquinaria.Codigo)
        {
            if (await maquinariaRepository.ExisteCodigoAsync(request.Codigo, id))
                return Result.Failure("Ya existe una máquina con ese código.", ErrorType.Conflict);
        }

        request.ApplyTo(maquinaria);
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var maquinaria = await maquinariaRepository.GetByIdAsync(id);

        if (maquinaria is null)
            return Result.Failure("Máquina no encontrada.", ErrorType.NotFound);

        if (!TieneAcceso(maquinaria))
            return Result.Failure("No tiene acceso a esta máquina.", ErrorType.Forbidden);

        maquinariaRepository.Delete(maquinaria);
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    private Alcance ObtenerAlcance()
    {
        var permiso = currentUser.Current!.Permisos
            .FirstOrDefault(p => p.Codigo == RecursoCodigo.Maquinaria);
        return permiso?.Alcance ?? Alcance.Propio;
    }

    private bool TieneAcceso(Maquinaria maquinaria)
    {
        var alcance = ObtenerAlcance();
        if (alcance == Alcance.Global) return true;
        if (alcance == Alcance.Seccion)
            return maquinaria.SeccionId == currentUser.Current!.SeccionId;
        return false;
    }
}
