using Granja.Domain.Enums;
using Granja.Services;
using Granja.Services.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Granja.Web.Controllers;

[ApiController]
[Route("api/reportes")]
public class ReportesController : ControllerBase
{
    private readonly IReporteService _reportes;

    public ReportesController(IReporteService reportes)
    {
        _reportes = reportes;
    }

    /// <summary>Ventas del período: fecha, producto, cantidad, precio, total y total general.</summary>
    [HttpGet("ventas")]
    [ProducesResponseType<ReporteVentasResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Ventas([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, [FromQuery] TipoProducto? tipo, CancellationToken ct)
    {
        return Ok(await _reportes.VentasAsync(desde, hasta, tipo, ct));
    }

    /// <summary>Leche total producida por día, litros transformados y litros no aprovechados.</summary>
    [HttpGet("leche-diaria")]
    [ProducesResponseType<ReporteLecheDiariaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LecheDiaria([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken ct)
    {
        return Ok(await _reportes.LecheDiariaAsync(desde, hasta, ct));
    }

    /// <summary>Producto elaborado por día y por tipo (queso y yogurt).</summary>
    [HttpGet("productodiario")]
    [ProducesResponseType<ReporteProductoDiarioResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProductoDiario([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken ct)
    {
        return Ok(await _reportes.ProductoDiarioAsync(desde, hasta, ct));
    }

    /// <summary>Por producto: total producido, total vendido, disponible y dañado.</summary>
    [HttpGet("producto-total")]
    [ProducesResponseType<ReporteProductoTotalResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ProductoTotal(CancellationToken ct)
    {
        return Ok(await _reportes.ProductoTotalAsync(ct));
    }

    /// <summary>Producto dañado por lote y por tipo, con fecha de vencimiento.</summary>
    [HttpGet("productodanado")]
    [ProducesResponseType<ReporteProductoDanadoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProductoDanado([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken ct)
    {
        return Ok(await _reportes.ProductoDanadoAsync(desde, hasta, ct));
    }

    /// <summary>Vacas: producción promedio, la que más y la que menos produce, y las que están por debajo del promedio.</summary>
    [HttpGet("vacas")]
    [ProducesResponseType<ReporteVacasResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Vacas([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken ct)
    {
        return Ok(await _reportes.VacasAsync(desde, hasta, ct));
    }
}
