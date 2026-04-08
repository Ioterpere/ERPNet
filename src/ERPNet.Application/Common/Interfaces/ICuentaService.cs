using ERPNet.Application.Common.DTOs;
using ERPNet.Domain.Filters;

namespace ERPNet.Application.Common.Interfaces;

public interface ICuentaService
{
    Task<Result<ListaPaginada<CuentaResponse>>> GetAllAsync(CuentaFilter filtro);
    Task<Result<CuentaResponse>> GetByIdAsync(int id);
    Task<Result<CuentaResponse>> CreateAsync(CreateCuentaRequest request);
    Task<Result> UpdateAsync(int id, UpdateCuentaRequest request);
    Task<Result> DeleteAsync(int id);
    Task<Result<List<ApunteContableResponse>>> GetExtractoAsync(int cuentaId, ExtractoFilter filtro);
    Task<Result<List<SaldoMensualResponse>>> GetSaldosAsync(int cuentaId, int anio);
    Task<Result<List<TipoDiarioResponse>>> GetTiposDiarioAsync();
    Task<Result<List<CentroCosteResponse>>> GetCentrosCostosAsync();
}
