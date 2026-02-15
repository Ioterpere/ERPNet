using ERPNet.Application.Common.DTOs;
using ERPNet.Domain.Filters;

namespace ERPNet.Application.Common.Interfaces;

public interface IMaquinariaService
{
    Task<Result<ListaPaginada<MaquinariaResponse>>> GetAllAsync(PaginacionFilter filtro);
    Task<Result<MaquinariaResponse>> GetByIdAsync(int id);
    Task<Result<MaquinariaResponse>> CreateAsync(CreateMaquinariaRequest request);
    Task<Result> UpdateAsync(int id, UpdateMaquinariaRequest request);
    Task<Result> DeleteAsync(int id);
}
