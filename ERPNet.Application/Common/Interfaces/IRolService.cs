using ERPNet.Contracts;
using ERPNet.Contracts.DTOs;
using ERPNet.Domain.Filters;

namespace ERPNet.Application.Common.Interfaces;

public interface IRolService
{
    Task<Result<ListaPaginada<RolResponse>>> GetAllAsync(PaginacionFilter filtro);
    Task<Result<RolResponse>> GetByIdAsync(int id);
    Task<Result<RolResponse>> CreateAsync(CreateRolRequest request);
    Task<Result> UpdateAsync(int id, UpdateRolRequest request);
    Task<Result> DeleteAsync(int id);
}
