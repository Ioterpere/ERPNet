using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.DTOs.Mappings;
using ERPNet.Application.Common.Enums;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;

namespace ERPNet.Application.Common;

public class RolService(
    IRolRepository rolRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache) : IRolService
{
    public async Task<Result<ListaPaginada<RolResponse>>> GetAllAsync(PaginacionFilter filtro)
    {
        var (roles, total) = await rolRepository.GetPaginatedAsync(filtro);
        var response = roles.Select(r => r.ToResponse()).ToList();
        return Result<ListaPaginada<RolResponse>>.Success(
            ListaPaginada<RolResponse>.Crear(response, total, filtro));
    }

    public async Task<Result<RolResponse>> GetByIdAsync(int id)
    {
        var rol = await rolRepository.GetByIdAsync(id);

        if (rol is null)
            return Result<RolResponse>.Failure("Rol no encontrado.", ErrorType.NotFound);

        return Result<RolResponse>.Success(rol.ToResponse());
    }

    public async Task<Result<RolResponse>> CreateAsync(CreateRolRequest request)
    {
        if (await rolRepository.ExisteNombreAsync(request.Nombre))
            return Result<RolResponse>.Failure("Ya existe un rol con ese nombre.", ErrorType.Conflict);

        var rol = request.ToEntity();

        await rolRepository.CreateAsync(rol);
        await unitOfWork.SaveChangesAsync();

        return Result<RolResponse>.Success(rol.ToResponse());
    }

    public async Task<Result> UpdateAsync(int id, UpdateRolRequest request)
    {
        var rol = await rolRepository.GetByIdAsync(id);

        if (rol is null)
            return Result.Failure("Rol no encontrado.", ErrorType.NotFound);

        if (request.Nombre is not null && request.Nombre != rol.Nombre)
        {
            if (await rolRepository.ExisteNombreAsync(request.Nombre, id))
                return Result.Failure("Ya existe un rol con ese nombre.", ErrorType.Conflict);
        }

        request.ApplyTo(rol);
        await unitOfWork.SaveChangesAsync();
        InvalidarCacheUsuarios(id);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var rol = await rolRepository.GetByIdAsync(id);

        if (rol is null)
            return Result.Failure("Rol no encontrado.", ErrorType.NotFound);

        rolRepository.Delete(rol);
        await unitOfWork.SaveChangesAsync();
        InvalidarCacheUsuarios(id);

        return Result.Success();
    }

    private void InvalidarCacheUsuarios(int rolId)
    {
        var usuarioIds = rolRepository.GetUsuarioIdsPorRol(rolId);
        foreach (var uid in usuarioIds)
            cache.Remove($"usuario:{uid}");
    }
}
