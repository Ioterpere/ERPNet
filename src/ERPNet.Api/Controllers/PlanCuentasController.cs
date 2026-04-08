using ERPNet.Api.Attributes;
using ERPNet.Api.Controllers.Common;
using ERPNet.Application.Common.DTOs;
using ERPNet.Application.Common.Interfaces;
using ERPNet.Domain.Enums;
using ERPNet.Domain.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ERPNet.Api.Controllers;

[Recurso(RecursoCodigo.Contabilidad)]
public class PlanCuentasController(ICuentaService cuentaService) : BaseController
{
    // ── Catálogos de apoyo ─────────────────────────────────────────
    [HttpGet("tipos-diario")]
    [ProducesResponseType<List<TipoDiarioResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTiposDiario()
        => FromResult(await cuentaService.GetTiposDiarioAsync());

    [HttpGet("centros-coste")]
    [ProducesResponseType<List<CentroCosteResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCentrosCoste()
        => FromResult(await cuentaService.GetCentrosCostosAsync());

    // ── CRUD de cuentas ────────────────────────────────────────────

    [HttpGet]
    [ProducesResponseType<ListaPaginada<CuentaResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] CuentaFilter filtro)
        => FromResult(await cuentaService.GetAllAsync(filtro));

    [HttpGet("{id}")]
    [ProducesResponseType<CuentaResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
        => FromResult(await cuentaService.GetByIdAsync(id));

    [HttpPost]
    [ProducesResponseType<CuentaResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCuentaRequest request)
        => CreatedFromResult(await cuentaService.CreateAsync(request));

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCuentaRequest request)
        => FromResult(await cuentaService.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
        => FromResult(await cuentaService.DeleteAsync(id));

    // ── Extracto de apuntes ────────────────────────────────────────

    [HttpGet("{id}/extracto")]
    [ProducesResponseType<List<ApunteContableResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExtracto(int id, [FromQuery] ExtractoFilter filtro)
        => FromResult(await cuentaService.GetExtractoAsync(id, filtro));

    // ── Saldos mensuales ───────────────────────────────────────────

    [HttpGet("{id}/saldos")]
    [ProducesResponseType<List<SaldoMensualResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSaldos(int id, [FromQuery] int anio)
        => FromResult(await cuentaService.GetSaldosAsync(id, anio));
}
