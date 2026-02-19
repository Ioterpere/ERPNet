using ERPNet.Application.Common.DTOs;
using ERPNet.Domain.Filters;

namespace ERPNet.Application.Common.Interfaces;

public interface IRolService
{
    Task<Result<ListaPaginada<RolResponse>>> GetAllAsync(PaginacionFilter filtro);
    Task<Result<RolResponse>> GetByIdAsync(int id);
    Task<Result<RolResponse>> CreateAsync(CreateRolRequest request);
    Task<Result> UpdateAsync(int id, UpdateRolRequest request);
    Task<Result> DeleteAsync(int id);
    Task<Result<IEnumerable<RecursoResponse>>> GetAllRecursosAsync();
    Task<Result<IEnumerable<PermisoRolRecursoResponse>>> GetPermisosAsync(int rolId);
    Task<Result> SetPermisosAsync(int rolId, SetPermisosRolRequest request);
    Task<Result<IEnumerable<UsuarioResponse>>> GetUsuariosAsync(int rolId);
    Task<Result> SetUsuariosAsync(int rolId, AsignarUsuariosRequest request);
}
