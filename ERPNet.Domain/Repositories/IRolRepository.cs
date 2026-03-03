using ERPNet.Domain.Entities;

namespace ERPNet.Domain.Repositories;

public interface IRolRepository : IRepository<Rol>
{
    Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null);
    Task<List<int>> GetUsuarioIdsPorRolAsync(int rolId);
    Task<IEnumerable<Recurso>> GetAllRecursosAsync();
    Task<IEnumerable<PermisoRolRecurso>> GetPermisosAsync(int rolId);
    Task SincronizarPermisosAsync(int rolId, IEnumerable<PermisoRolRecurso> nuevos);
    Task<List<(int UsuarioId, int? EmpresaId)>> GetTodasAsignacionesUsuarioAsync(int rolId);
    Task SincronizarTodasAsignacionesUsuarioAsync(int rolId, List<(int UsuarioId, int? EmpresaId)> asignaciones);
}
