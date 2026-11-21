using Granja.Domain.Enums;
using Granja.Services;
using Granja.Services.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Granja.Web.Controllers;

[ApiController]
[Route("api/inventario")]
public class InventarioController : ControllerBase
{
    private readonly IInventarioService _inventario;

    public InventarioController(IInventarioService inventario)
    {
        _inventario = inventario;
    }

    /// <summary>Consulta el inventario por lotes y el resumen por producto a una fecha.</summary>
    [HttpGet]
    [ProducesResponseType<InventarioResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Obtener([FromQuery] DateOnly? fecha, [FromQuery] TipoProducto? tipo, CancellationToken ct)
    {
        return Ok(await _inventario.ObtenerInventarioAsync(fecha, tipo, ct));
    }

    /// <summary>Procesa vencimientos: la cantidad disponible de los lotes vencidos pasa a dañada.</summary>
    [HttpPost("vencimientos")]
    [ProducesResponseType<ProcesarVencimientosResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcesarVencimientos([FromQuery] DateOnly? fecha, CancellationToken ct)
    {
        return Ok(await _inventario.ProcesarVencimientosAsync(fecha, ct));
    }

    /// <summary>Alerta de lotes con existencias que vencen en los próximos días (por defecto 3).</summary>
    [HttpGet("por-vencer")]
    [ProducesResponseType<IReadOnlyList<LoteResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PorVencer([FromQuery] int dias = 3, [FromQuery] DateOnly? fecha = null, CancellationToken ct = default)
    {
        return Ok(await _inventario.ObtenerPorVencerAsync(dias, fecha, ct));
    }
}
