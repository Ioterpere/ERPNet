using ERPNet.Domain.Entities;

namespace ERPNet.Domain.Repositories;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> GetByIdConPermisosAsync(int id);
    Task<Usuario?> GetByEmailAsync(string email);
    Task<bool> ExisteEmailAsync(string email, int? excluirId = null);
    Task UpdateUltimoAccesoAsync(int usuarioId, DateTime fecha);
    Task<List<string>> GetEmailsByRolAsync(string nombreRol);
}
