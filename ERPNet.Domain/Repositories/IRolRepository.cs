using ERPNet.Domain.Entities;

namespace ERPNet.Domain.Repositories;

public interface IRolRepository : IRepository<Rol>
{
    Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null);
    List<int> GetUsuarioIdsPorRol(int rolId);
    Task<IEnumerable<Recurso>> GetAllRecursosAsync();
    Task<IEnumerable<PermisoRolRecurso>> GetPermisosAsync(int rolId);
    Task SincronizarPermisosAsync(int rolId, IEnumerable<PermisoRolRecurso> nuevos);
    Task SincronizarUsuariosAsync(int rolId, List<int> usuarioIds);
    Task<IEnumerable<Usuario>> GetUsuariosAsync(int rolId);
}
