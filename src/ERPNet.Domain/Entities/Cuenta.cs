using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class Cuenta : BaseEntity, IPerteneceEmpresa
{
    // ── Identificación ─────────────────────────────────────────────
    public string Codigo { get; set; } = null!;          // max 9, e.g. "10000000"
    public string Descripcion { get; set; } = null!;     // max 60

    // ── Datos adicionales ──────────────────────────────────────────
    public string? DescripcionSII { get; set; }          // max 60
    public string? Nif { get; set; }                     // max 15

    // ── Flags ──────────────────────────────────────────────────────
    public bool EsNoOficial { get; set; }                // cuenta agrupadora/organizativa
    public bool EsObsoleta { get; set; }

    // ── Empresa ────────────────────────────────────────────────────
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    // ── Jerarquía (auto-ref) ───────────────────────────────────────
    public int? CuentaPadreId { get; set; }
    public Cuenta? CuentaPadre { get; set; }
    public ICollection<Cuenta> CuentasHijas { get; set; } = [];

    // ── Relaciones asociadas (legacy: parámetros de cuenta) ──────
    public int? CuentaAmortizacionId { get; set; }          // Cta Amort. Asociada (Gasto)
    public Cuenta? CuentaAmortizacion { get; set; }

    public int? CuentaPagoDelegadoId { get; set; }          // Cta Pago Delegado
    public Cuenta? CuentaPagoDelegado { get; set; }

    public int? EmpresaVinculadaId { get; set; }            // Empresa intercompany
    public Empresa? EmpresaVinculada { get; set; }

    public int? ConceptoAnaliticaId { get; set; }           // FK futura a contanaconceptos
    public int? ClienteAsociadoId { get; set; }             // FK futura a clientes

    // ── Apuntes ────────────────────────────────────────────────────
    public ICollection<ApunteContable> Apuntes { get; set; } = [];
}
