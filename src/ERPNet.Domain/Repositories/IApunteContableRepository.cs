using ERPNet.Domain.Entities;
using ERPNet.Domain.Filters;

namespace ERPNet.Domain.Repositories;

public record SaldoMensual(int Mes, decimal Debe, decimal Haber, int NumApuntes);

public interface IApunteContableRepository : IRepository<ApunteContable>
{
    Task<List<ApunteContable>> GetExtractoAsync(int cuentaId, ExtractoFilter filtro);
    Task<List<SaldoMensual>> GetSaldosMensualesAsync(int cuentaId, int anio);
}
