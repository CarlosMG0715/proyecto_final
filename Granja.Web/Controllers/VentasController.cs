using Granja.Domain.Enums;
using Granja.Services;
using Granja.Services.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Granja.Web.Controllers;

[ApiController]
[Route("api/ventas")]
public class VentasController : ControllerBase
{
    private readonly IVentaService _ventas;

    public VentasController(IVentaService ventas)
    {
        _ventas = ventas;
    }

    /// <summary>Registra una venta despachando siempre el producto más antiguo no vencido (FIFO).</summary>
    [HttpPost]
    [ProducesResponseType<VentaResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar([FromBody] VentaRequest request, CancellationToken ct)
    {
        var venta = await _ventas.RegistrarVentaAsync(request, ct);
        return CreatedAtAction(nameof(Obtener), new { id = venta.Id }, venta);
    }

    /// <summary>Consulta una venta con el detalle de los lotes de los que salió el producto.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<VentaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener(int id, CancellationToken ct)
    {
        return Ok(await _ventas.ObtenerAsync(id, ct));
    }

    /// <summary>Lista las ventas, con filtros opcionales por fecha y tipo.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<VentaResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta,
        [FromQuery] TipoProducto? tipo,
        CancellationToken ct)
    {
        return Ok(await _ventas.ListarAsync(desde, hasta, tipo, ct));
    }
}
