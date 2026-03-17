using ERPNet.Domain.Entities;

namespace ERPNet.Domain.Repositories;

public interface IEmpresaRepository : IRepository<Empresa>
{
    Task<List<Empresa>> GetEmpresasDeUsuarioAsync(int usuarioId);
    Task SincronizarEmpresasDeUsuarioAsync(int usuarioId, List<int> empresaIds);
}
