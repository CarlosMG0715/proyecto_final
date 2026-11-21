using Granja.Services;
using Granja.Services.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Granja.Web.Controllers;

[ApiController]
[Route("api/produccion")]
public class ProduccionController : ControllerBase
{
    private readonly IProduccionService _produccion;

    public ProduccionController(IProduccionService produccion)
    {
        _produccion = produccion;
    }

    /// <summary>Registra la producción de leche del día (n × l litros), una sola vez por fecha.</summary>
    [HttpPost]
    [ProducesResponseType<ProduccionDiaResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarProduccionRequest request, CancellationToken ct)
    {
        var resultado = await _produccion.RegistrarProduccionAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, resultado);
    }

    /// <summary>Lista la producción de leche por día en un rango de fechas.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProduccionDiaResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken ct)
    {
        return Ok(await _produccion.ListarProduccionAsync(desde, hasta, ct));
    }

    /// <summary>Transforma leche de una fecha en un lote de queso o yogurt.</summary>
    [HttpPost("transformaciones")]
    [ProducesResponseType<TransformacionResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Transformar([FromBody] TransformarLecheRequest request, CancellationToken ct)
    {
        var resultado = await _produccion.TransformarAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, resultado);
    }
}
