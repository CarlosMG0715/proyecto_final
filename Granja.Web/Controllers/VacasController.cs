using Granja.Services;
using Granja.Services.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Granja.Web.Controllers;

[ApiController]
[Route("api/vacas")]
public class VacasController : ControllerBase
{
    private readonly IGranjaService _granja;

    public VacasController(IGranjaService granja)
    {
        _granja = granja;
    }

    /// <summary>Lista las vacas de la granja (útil para registrar producción individual).</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<VacaResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        return Ok(await _granja.ListarVacasAsync(ct));
    }
}
