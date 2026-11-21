using System.ComponentModel.DataAnnotations;
using Granja.Domain.Enums;

namespace Granja.Services.Dtos;

public class ProduccionVacaRequest
{
    public int VacaId { get; set; }

    [Range(0, 100000)]
    public decimal Litros { get; set; }
}

public class RegistrarProduccionRequest
{
    /// <summary>Fecha de la producción (yyyy-MM-dd). Por defecto, la fecha actual del IClock.</summary>
    public DateOnly? Fecha { get; set; }

    /// <summary>
    /// Opcional: litros específicos de algunas vacas. Las vacas que no se envíen producen l litros (RN-01).
    /// </summary>
    public List<ProduccionVacaRequest>? Vacas { get; set; }
}

public class ProduccionVacaResponse
{
    public int VacaId { get; set; }
    public string Vaca { get; set; } = string.Empty;
    public decimal Litros { get; set; }
}

public class ProduccionDiaResponse
{
    public DateOnly Fecha { get; set; }
    public int NumeroVacas { get; set; }
    public decimal TotalLitros { get; set; }
    public List<ProduccionVacaResponse> Detalle { get; set; } = new();
}

public class TransformarLecheRequest
{
    /// <summary>Fecha de la leche que se transforma (yyyy-MM-dd). Por defecto, la fecha actual.</summary>
    public DateOnly? Fecha { get; set; }

    [EnumDataType(typeof(TipoProducto))]
    public TipoProducto TipoProducto { get; set; }

    [Range(0.01, 10000000)]
    public decimal Litros { get; set; }
}

public class TransformacionResponse
{
    public LoteResponse Lote { get; set; } = new();
    public decimal LitrosDisponiblesRestantes { get; set; }
}
