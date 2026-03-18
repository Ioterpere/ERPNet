using ERPNet.Domain.Entities;

namespace ERPNet.Domain.Repositories;

public interface IFamiliaArticuloRepository : IRepository<FamiliaArticulo>
{
    Task<List<FamiliaArticulo>> GetAllByEmpresaAsync(int empresaId);
}
