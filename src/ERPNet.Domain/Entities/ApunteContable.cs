using ERPNet.Domain.Common;

namespace ERPNet.Domain.Entities;

public class ApunteContable : BaseEntity, IPerteneceEmpresa
{
    // ── Claves foráneas ────────────────────────────────────────────
    public int CuentaId { get; set; }
    public int? TipoDiarioId { get; set; }
    public int? CentroCosteId { get; set; }
    public int EmpresaId { get; set; }

    // ── Asiento ────────────────────────────────────────────────────
    public int Asiento { get; set; }    // agrupa apuntes; regla: sum(Debe)==sum(Haber)
    public int NumLinea { get; set; }
    public int NumDiario { get; set; }

    // ── Datos contables ────────────────────────────────────────────
    public DateOnly Fecha { get; set; }
    public string Concepto { get; set; } = null!; // max 40
    public decimal Debe { get; set; }             // precision 10,2
    public decimal Haber { get; set; }            // precision 10,2
    public bool EsDefinitivo { get; set; }

    // ── Punteo ─────────────────────────────────────────────────────
    public int? IdPunteo { get; set; }
    public DateOnly? FechaPunteo { get; set; }

    // ── Navegaciones ───────────────────────────────────────────────
    public Cuenta Cuenta { get; set; } = null!;
    public TipoDiario? TipoDiario { get; set; }
    public CentroCoste? CentroCoste { get; set; }
    public Empresa Empresa { get; set; } = null!;
}
