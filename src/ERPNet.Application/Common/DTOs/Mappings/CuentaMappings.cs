using ERPNet.Domain.Entities;
using ERPNet.Domain.Repositories;

namespace ERPNet.Application.Common.DTOs.Mappings;

public static class CuentaMappings
{
    public static CuentaResponse ToResponse(this Cuenta cuenta) => new()
    {
        Id                    = cuenta.Id,
        Codigo                = cuenta.Codigo,
        Descripcion           = cuenta.Descripcion,
        DescripcionSII        = cuenta.DescripcionSII,
        Nif                   = cuenta.Nif,
        EsNoOficial           = cuenta.EsNoOficial,
        EsObsoleta            = cuenta.EsObsoleta,
        EmpresaId             = cuenta.EmpresaId,
        CuentaPadreId         = cuenta.CuentaPadreId,
        CuentaPadreCodigo          = cuenta.CuentaPadre?.Codigo,
        CuentaPadreDescripcion     = cuenta.CuentaPadre?.Descripcion,
        CuentaPadreDescripcionSII  = cuenta.CuentaPadre?.DescripcionSII,

        CuentaAmortizacionId          = cuenta.CuentaAmortizacionId,
        CuentaAmortizacionCodigo      = cuenta.CuentaAmortizacion?.Codigo,
        CuentaAmortizacionDescripcion = cuenta.CuentaAmortizacion?.Descripcion,

        CuentaPagoDelegadoId          = cuenta.CuentaPagoDelegadoId,
        CuentaPagoDelegadoCodigo      = cuenta.CuentaPagoDelegado?.Codigo,
        CuentaPagoDelegadoDescripcion = cuenta.CuentaPagoDelegado?.Descripcion,

        EmpresaVinculadaId     = cuenta.EmpresaVinculadaId,
        EmpresaVinculadaNombre = cuenta.EmpresaVinculada?.Nombre,

        ConceptoAnaliticaId = cuenta.ConceptoAnaliticaId,
        ClienteAsociadoId   = cuenta.ClienteAsociadoId,
    };

    public static Cuenta ToEntity(this CreateCuentaRequest request, int empresaId) => new()
    {
        Codigo        = request.Codigo,
        Descripcion   = request.Descripcion,
        DescripcionSII = string.IsNullOrWhiteSpace(request.DescripcionSII) ? null : request.DescripcionSII,
        Nif           = string.IsNullOrWhiteSpace(request.Nif) ? null : request.Nif,
        EsNoOficial   = request.EsNoOficial,
        EsObsoleta    = request.EsObsoleta,
        EmpresaId     = empresaId,
        CuentaPadreId = request.CuentaPadreId,
    };

    public static void ApplyTo(this UpdateCuentaRequest request, Cuenta cuenta)
    {
        if (request.Descripcion is not null)
            cuenta.Descripcion = request.Descripcion;

        if (request.DescripcionSII is not null)
            cuenta.DescripcionSII = string.IsNullOrWhiteSpace(request.DescripcionSII) ? null : request.DescripcionSII;

        if (request.Nif is not null)
            cuenta.Nif = string.IsNullOrWhiteSpace(request.Nif) ? null : request.Nif;

        if (request.EsNoOficial.HasValue)
            cuenta.EsNoOficial = request.EsNoOficial.Value;

        if (request.EsObsoleta.HasValue)
            cuenta.EsObsoleta = request.EsObsoleta.Value;

        if (request.CuentaPadreId.HasValue)
            cuenta.CuentaPadreId = request.CuentaPadreId.Value == 0 ? null : request.CuentaPadreId;

        if (request.CuentaAmortizacionId.HasValue)
            cuenta.CuentaAmortizacionId = request.CuentaAmortizacionId.Value == 0 ? null : request.CuentaAmortizacionId;

        if (request.CuentaPagoDelegadoId.HasValue)
            cuenta.CuentaPagoDelegadoId = request.CuentaPagoDelegadoId.Value == 0 ? null : request.CuentaPagoDelegadoId;

        if (request.EmpresaVinculadaId.HasValue)
            cuenta.EmpresaVinculadaId = request.EmpresaVinculadaId.Value == 0 ? null : request.EmpresaVinculadaId;

        if (request.ConceptoAnaliticaId.HasValue)
            cuenta.ConceptoAnaliticaId = request.ConceptoAnaliticaId.Value == 0 ? null : request.ConceptoAnaliticaId;

        if (request.ClienteAsociadoId.HasValue)
            cuenta.ClienteAsociadoId = request.ClienteAsociadoId.Value == 0 ? null : request.ClienteAsociadoId;
    }

    public static ApunteContableResponse ToResponse(this ApunteContable apunte) => new()
    {
        Id                    = apunte.Id,
        CuentaId              = apunte.CuentaId,
        CodigoCuenta          = apunte.Cuenta?.Codigo ?? string.Empty,
        TipoDiarioId          = apunte.TipoDiarioId,
        CodigoTipoDiario      = apunte.TipoDiario?.Codigo,
        DescripcionTipoDiario = apunte.TipoDiario?.Descripcion,
        CentroCosteId         = apunte.CentroCosteId,
        CodigoCentroCoste     = apunte.CentroCoste?.Codigo,
        DescripcionCentroCoste = apunte.CentroCoste?.Descripcion,
        Asiento               = apunte.Asiento,
        NumLinea              = apunte.NumLinea,
        NumDiario             = apunte.NumDiario,
        Fecha                 = apunte.Fecha,
        Concepto              = apunte.Concepto,
        Debe                  = apunte.Debe,
        Haber                 = apunte.Haber,
        EsDefinitivo          = apunte.EsDefinitivo,
        IdPunteo              = apunte.IdPunteo,
        FechaPunteo           = apunte.FechaPunteo,
    };

    public static TipoDiarioResponse ToResponse(this TipoDiario tipoDiario) => new()
    {
        Id          = tipoDiario.Id,
        Codigo      = tipoDiario.Codigo,
        Descripcion = tipoDiario.Descripcion,
        EsNoOficial = tipoDiario.EsNoOficial,
    };

    public static CentroCosteResponse ToResponse(this CentroCoste centroCoste) => new()
    {
        Id          = centroCoste.Id,
        Codigo      = centroCoste.Codigo,
        Descripcion = centroCoste.Descripcion,
    };

    public static SaldoMensualResponse ToResponse(this SaldoMensual saldo, decimal saldoAcumulado) => new()
    {
        Mes            = saldo.Mes,
        Debe           = saldo.Debe,
        Haber          = saldo.Haber,
        SaldoMes       = saldo.Debe - saldo.Haber,
        SaldoAcumulado = saldoAcumulado,
        NumApuntes     = saldo.NumApuntes,
    };
}
