using System.ComponentModel.DataAnnotations;
using Granja.Domain.Enums;

namespace Granja.Services.Dtos;

public class ParametroProductoDto
{
    [EnumDataType(typeof(TipoProducto))]
    public TipoProducto Tipo { get; set; }

    /// <summary>Litros de leche por unidad de producto (queso: 10 → 10 L = 1 kg).</summary>
    [Range(0.0001, 1000000)]
    public decimal Rendimiento { get; set; }

    [Range(1, 3650)]
    public int VidaUtilDias { get; set; }

    [Range(0, 1000000000)]
    public decimal Precio { get; set; }
}

public class GranjaResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int NumeroVacas { get; set; }
    public decimal LitrosPorVaca { get; set; }
    public decimal ProduccionDiariaEsperada { get; set; }
    public List<ParametroProductoDto> Parametros { get; set; } = new();
}

public class ActualizarGranjaRequest
{
    [Range(1, 100000)]
    public int NumeroVacas { get; set; }

    [Range(0.01, 100000)]
    public decimal LitrosPorVaca { get; set; }

    /// <summary>Opcional: solo se actualizan los productos que se envíen.</summary>
    public List<ParametroProductoDto>? Parametros { get; set; }
}

public class VacaResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activa { get; set; }
}
