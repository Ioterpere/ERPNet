using ERPNet.Application.Common.DTOs;
using ERPNet.Domain.Filters;

namespace ERPNet.Application.Common.Interfaces;

public interface IArticuloService
{
    Task<Result<ListaPaginada<ArticuloResponse>>> GetAllAsync(PaginacionFilter filtro);
    Task<Result<ArticuloResponse>> GetByIdAsync(int id);
    Task<Result<ArticuloResponse>> CreateAsync(CreateArticuloRequest request);
    Task<Result> UpdateAsync(int id, UpdateArticuloRequest request);
    Task<Result> DeleteAsync(int id);
    Task<Result<List<ArticuloLogResponse>>> GetLogsAsync(int articuloId);
    Task<Result<ArticuloLogResponse>> AddLogAsync(int articuloId, CreateArticuloLogRequest request);
    Task<Result<List<FamiliaArticuloResponse>>> GetFamiliasAsync();
    Task<Result<List<TipoIvaResponse>>> GetTiposIvaAsync();
    Task<Result<List<FormatoArticuloResponse>>> GetFormatosAsync();
}
