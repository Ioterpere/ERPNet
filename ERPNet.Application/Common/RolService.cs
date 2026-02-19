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
        InvalidarCacheUsuarios(rolId);

        return Result.Success();
    }

    public async Task<Result<IEnumerable<UsuarioResponse>>> GetUsuariosAsync(int rolId)
    {
        var rol = await rolRepository.GetByIdAsync(rolId);

        if (rol is null)
            return Result<IEnumerable<UsuarioResponse>>.Failure("Rol no encontrado.", ErrorType.NotFound);

        var usuarios = await rolRepository.GetUsuariosAsync(rolId);
        return Result<IEnumerable<UsuarioResponse>>.Success(usuarios.Select(u => u.ToResponse()));
    }

    public async Task<Result> SetUsuariosAsync(int rolId, AsignarUsuariosRequest request)
    {
        var rol = await rolRepository.GetByIdAsync(rolId);

        if (rol is null)
            return Result.Failure("Rol no encontrado.", ErrorType.NotFound);

        await rolRepository.SincronizarUsuariosAsync(rolId, request.UsuarioIds);
        await unitOfWork.SaveChangesAsync();
        InvalidarCacheUsuarios(rolId);

        return Result.Success();
    }

    private void InvalidarCacheUsuarios(int rolId)
    {
        var usuarioIds = rolRepository.GetUsuarioIdsPorRol(rolId);
        foreach (var uid in usuarioIds)
            cache.Remove($"usuario:{uid}");
    }
}
