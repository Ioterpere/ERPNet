using ERPNet.Domain.Entities;

namespace ERPNet.Domain.Repositories;

public interface IUsuarioRepository
{
    Task<List<Usuario>> GetAllAsync();
    Task<Usuario?> GetByIdAsync(int id);
    Task<Usuario?> GetByIdConPermisosAsync(int id);
    Task<Usuario?> GetByEmailAsync(string email);
    Task<bool> ExisteEmailAsync(string email, int? excluirId = null);
    Task<Usuario> CreateAsync(Usuario usuario);
    Task UpdateAsync(Usuario usuario);
    Task SoftDeleteAsync(int id, int deletedBy);
    Task UpdateUltimoAccesoAsync(int usuarioId, DateTime fecha);
}
