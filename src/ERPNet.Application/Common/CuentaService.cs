using ERPNet.Application.Auth.Interfaces;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.DTOs.Mappings;
using ERPNet.Application.Common.Enums;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Filters;
using ERPNet.Domain.Repositories;

namespace ERPNet.Application.Common;

public class CuentaService(
    ICuentaRepository cuentaRepository,
    IApunteContableRepository apunteRepository,
    ITipoDiarioRepository tipoDiarioRepository,
    ICentroCosteRepository centroCosteRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserProvider currentUser) : ICuentaService
{
    public async Task<Result<ListaPaginada<CuentaResponse>>> GetAllAsync(CuentaFilter filtro)
    {
        var (cuentas, total) = await cuentaRepository.GetPaginatedAsync(filtro);
        var response = cuentas.Select(c => c.ToResponse()).ToList();
        return Result<ListaPaginada<CuentaResponse>>.Success(
            ListaPaginada<CuentaResponse>.Crear(response, total));
    }

    public async Task<Result<CuentaResponse>> GetByIdAsync(int id)
    {
        var cuenta = await cuentaRepository.GetByIdAsync(id);

        if (cuenta is null)
            return Result<CuentaResponse>.Failure("Cuenta no encontrada.", ErrorType.NotFound);

        return Result<CuentaResponse>.Success(cuenta.ToResponse());
    }

    public async Task<Result<CuentaResponse>> CreateAsync(CreateCuentaRequest request)
    {
        var empresaId = currentUser.Current!.EmpresaId ?? 0;

        if (await cuentaRepository.ExisteCodigoAsync(request.Codigo, empresaId))
            return Result<CuentaResponse>.Failure("Ya existe una cuenta con ese código.", ErrorType.Conflict);

        var cuenta = request.ToEntity(empresaId);
        await cuentaRepository.CreateAsync(cuenta);
        await unitOfWork.SaveChangesAsync();

        return Result<CuentaResponse>.Success(cuenta.ToResponse());
    }

    public async Task<Result> UpdateAsync(int id, UpdateCuentaRequest request)
    {
        var cuenta = await cuentaRepository.GetByIdAsync(id);

        if (cuenta is null)
            return Result.Failure("Cuenta no encontrada.", ErrorType.NotFound);

        if (request.CuentaPadreId.HasValue && request.CuentaPadreId.Value == id)
            return Result.Failure("Una cuenta no puede ser su propia cuenta padre.", ErrorType.Validation);

        request.ApplyTo(cuenta);
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var cuenta = await cuentaRepository.GetByIdAsync(id);

        if (cuenta is null)
            return Result.Failure("Cuenta no encontrada.", ErrorType.NotFound);

        cuentaRepository.Delete(cuenta);
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<List<ApunteContableResponse>>> GetExtractoAsync(int cuentaId, ExtractoFilter filtro)
    {
        var cuenta = await cuentaRepository.GetByIdAsync(cuentaId);

        if (cuenta is null)
            return Result<List<ApunteContableResponse>>.Failure("Cuenta no encontrada.", ErrorType.NotFound);

        var apuntes = await apunteRepository.GetExtractoAsync(cuentaId, filtro);
        var response = apuntes.Select(a => a.ToResponse()).ToList();

        return Result<List<ApunteContableResponse>>.Success(response);
    }

    public async Task<Result<List<SaldoMensualResponse>>> GetSaldosAsync(int cuentaId, int anio)
    {
        var cuenta = await cuentaRepository.GetByIdAsync(cuentaId);

        if (cuenta is null)
            return Result<List<SaldoMensualResponse>>.Failure("Cuenta no encontrada.", ErrorType.NotFound);

        var saldos = await apunteRepository.GetSaldosMensualesAsync(cuentaId, anio);

        // Calcular saldo acumulado
        decimal acumulado = 0;
        var response = saldos.Select(s =>
        {
            acumulado += s.Debe - s.Haber;
            return s.ToResponse(acumulado);
        }).ToList();

        return Result<List<SaldoMensualResponse>>.Success(response);
    }

    public async Task<Result<List<TipoDiarioResponse>>> GetTiposDiarioAsync()
    {
        var tipos = await tipoDiarioRepository.GetAllOrdenadosAsync();
        return Result<List<TipoDiarioResponse>>.Success(tipos.Select(t => t.ToResponse()).ToList());
    }

    public async Task<Result<List<CentroCosteResponse>>> GetCentrosCostosAsync()
    {
        var centros = await centroCosteRepository.GetAllOrdenadosAsync();
        return Result<List<CentroCosteResponse>>.Success(centros.Select(c => c.ToResponse()).ToList());
    }
}
