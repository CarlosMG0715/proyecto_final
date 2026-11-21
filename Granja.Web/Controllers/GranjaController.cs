using Granja.Services;
using Granja.Services.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Granja.Web.Controllers;

[ApiController]
[Route("api/granja")]
public class GranjaController : ControllerBase
{
    private readonly IGranjaService _granja;

    public GranjaController(IGranjaService granja)
    {
        _granja = granja;
    }

    /// <summary>Consulta la configuración (n, l) y los parámetros por producto.</summary>
    [HttpGet]
    [ProducesResponseType<GranjaResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Obtener(CancellationToken ct)
    {
        return Ok(await _granja.ObtenerAsync(ct));
    }

    /// <summary>Actualiza n, l y los parámetros de rendimiento, vida útil y precio.</summary>
    [HttpPut]
    [ProducesResponseType<GranjaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Actualizar([FromBody] ActualizarGranjaRequest request, CancellationToken ct)
    {
        return Ok(await _granja.ActualizarAsync(request, ct));
    }
}
