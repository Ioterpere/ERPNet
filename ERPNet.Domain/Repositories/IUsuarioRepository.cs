using ERPNet.Domain.Entities;

namespace ERPNet.Domain.Repositories;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> GetByIdConPermisosAsync(int id);
    Task<Usuario?> GetByEmailAsync(string email);
    Task<bool> ExisteEmailAsync(string email, int? excluirId = null);
    Task<bool> ExisteEmpleadoAsync(int empleadoId, int? excluirId = null);
    Task UpdateUltimoAccesoAsync(int usuarioId, DateTime fecha);
    Task<List<string>> GetEmailsByRolAsync(string nombreRol);
    Task<List<(int RolId, int? EmpresaId)>> GetTodasAsignacionesRolAsync(int usuarioId);
    Task<List<Rol>> GetRolesConNombreAsync(int usuarioId);
    Task SincronizarRolesAsync(int usuarioId, List<int> rolIds, int? empresaId = null);
    Task SincronizarTodasAsignacionesRolAsync(int usuarioId, List<(int RolId, int? EmpresaId)> asignaciones);
}
