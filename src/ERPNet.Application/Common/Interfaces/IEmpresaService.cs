using ERPNet.Application.Common.DTOs;
using ERPNet.Domain.Filters;

namespace ERPNet.Application.Common.Interfaces;

public interface IEmpresaService
{
    Task<Result<ListaPaginada<EmpresaResponse>>> GetAllAsync(PaginacionFilter filtro);
    Task<Result<EmpresaResponse>> GetByIdAsync(int id);
    Task<Result<EmpresaResponse>> CreateAsync(CreateEmpresaRequest request);
    Task<Result> UpdateAsync(int id, UpdateEmpresaRequest request);
    Task<Result> DeleteAsync(int id);
    Task<Result<IEnumerable<EmpresaResponse>>> GetEmpresasDeUsuarioAsync(int usuarioId);
    Task<Result> SincronizarEmpresasDeUsuarioAsync(int usuarioId, AsignarEmpresasRequest request);
}
