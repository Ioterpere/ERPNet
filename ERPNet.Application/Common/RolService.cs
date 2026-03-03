using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.DTOs.Mappings;
using ERPNet.Application.Common.Enums;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
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
        await InvalidarCacheUsuariosAsync(id);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var rol = await rolRepository.GetByIdAsync(id);

        if (rol is null)
            return Result.Failure("Rol no encontrado.", ErrorType.NotFound);

        rolRepository.Delete(rol);
        await unitOfWork.SaveChangesAsync();
        await InvalidarCacheUsuariosAsync(id);

        return Result.Success();
    }

    public async Task<Result<IEnumerable<RecursoResponse>>> GetAllRecursosAsync()
    {
        var recursos = await rolRepository.GetAllRecursosAsync();
        var response = recursos.Select(r => new RecursoResponse { Id = r.Id, Codigo = r.Codigo });
        return Result<IEnumerable<RecursoResponse>>.Success(response);
    }

    public async Task<Result<IEnumerable<PermisoRolRecursoResponse>>> GetPermisosAsync(int rolId)
    {
        var rol = await rolRepository.GetByIdAsync(rolId);

        if (rol is null)
            return Result<IEnumerable<PermisoRolRecursoResponse>>.Failure("Rol no encontrado.", ErrorType.NotFound);

        var permisos = await rolRepository.GetPermisosAsync(rolId);
        return Result<IEnumerable<PermisoRolRecursoResponse>>.Success(permisos.Select(p => p.ToResponse()));
    }

    public async Task<Result> SetPermisosAsync(int rolId, SetPermisosRolRequest request)
    {
        var rol = await rolRepository.GetByIdAsync(rolId);

        if (rol is null)
            return Result.Failure("Rol no encontrado.", ErrorType.NotFound);

        var nuevos = request.Permisos.Select(dto => new PermisoRolRecurso
        {
            RolId = rolId,
            RecursoId = dto.RecursoId,
            CanCreate = dto.CanCreate,
            CanEdit = dto.CanEdit,
            CanDelete = dto.CanDelete,
            Alcance = (Alcance)dto.Alcance
        });

        await rolRepository.SincronizarPermisosAsync(rolId, nuevos);
        await unitOfWork.SaveChangesAsync();
        await InvalidarCacheUsuariosAsync(rolId);

        return Result.Success();
    }

    public async Task<Result<List<AsignacionUsuarioDto>>> GetTodasAsignacionesUsuarioAsync(int rolId)
    {
        var rol = await rolRepository.GetByIdAsync(rolId);

        if (rol is null)
            return Result<List<AsignacionUsuarioDto>>.Failure("Rol no encontrado.", ErrorType.NotFound);

        var asignaciones = await rolRepository.GetTodasAsignacionesUsuarioAsync(rolId);
        return Result<List<AsignacionUsuarioDto>>.Success(
            asignaciones.Select(a => new AsignacionUsuarioDto { UsuarioId = a.UsuarioId, EmpresaId = a.EmpresaId }).ToList());
    }

    public async Task<Result> SincronizarTodasAsignacionesUsuarioAsync(int rolId, List<AsignacionUsuarioDto> asignaciones)
    {
        var rol = await rolRepository.GetByIdAsync(rolId);

        if (rol is null)
            return Result.Failure("Rol no encontrado.", ErrorType.NotFound);

        var idsAnteriores = await rolRepository.GetUsuarioIdsPorRolAsync(rolId);

        await rolRepository.SincronizarTodasAsignacionesUsuarioAsync(
            rolId, asignaciones.Select(a => (a.UsuarioId, a.EmpresaId)).ToList());
        await unitOfWork.SaveChangesAsync();

        foreach (var uid in idsAnteriores.Union(asignaciones.Select(a => a.UsuarioId)))
            cache.RemoveByPrefix($"usuario:{uid}:");

        return Result.Success();
    }

    private async Task InvalidarCacheUsuariosAsync(int rolId)
    {
        var usuarioIds = await rolRepository.GetUsuarioIdsPorRolAsync(rolId);
        foreach (var uid in usuarioIds)
            cache.RemoveByPrefix($"usuario:{uid}:");
    }
}
