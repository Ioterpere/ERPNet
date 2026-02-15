using ERPNet.Domain.Entities;

namespace ERPNet.Domain.Repositories;

public interface IRolRepository : IRepository<Rol>
{
    Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null);
    List<int> GetUsuarioIdsPorRol(int rolId);
}
