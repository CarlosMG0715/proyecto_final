using Granja.Domain.Enums;

namespace Granja.Services.Dtos;

public class LoteResponse
{
    public int Id { get; set; }
    public TipoProducto Tipo { get; set; }
    public string Unidad { get; set; } = string.Empty;
    public DateOnly FechaProduccion { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public decimal LitrosUsados { get; set; }
    public decimal CantidadInicial { get; set; }
    public decimal CantidadDisponible { get; set; }
    public decimal CantidadDanada { get; set; }
    public EstadoLote Estado { get; set; }
    public int DiasParaVencer { get; set; }
}

public class ResumenInventarioItem
{
    public TipoProducto Tipo { get; set; }
    public string Unidad { get; set; } = string.Empty;

    /// <summary>Existencias no vencidas a la fecha consultada.</summary>
    public decimal Vendible { get; set; }

    /// <summary>Existencias ya vencidas que aún no se han procesado como dañadas.</summary>
    public decimal VencidoSinProcesar { get; set; }

    public decimal Danado { get; set; }
}

public class InventarioResponse
{
    public DateOnly Fecha { get; set; }
    public List<ResumenInventarioItem> Resumen { get; set; } = new();
    public List<LoteResponse> Lotes { get; set; } = new();
}

public class PerdidaLoteResponse
{
    public int LoteId { get; set; }
    public TipoProducto Tipo { get; set; }
    public string Unidad { get; set; } = string.Empty;
    public DateOnly FechaProduccion { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public decimal CantidadDanada { get; set; }
}

public class ProcesarVencimientosResponse
{
    public DateOnly Fecha { get; set; }
    public int LotesProcesados { get; set; }
    public List<PerdidaLoteResponse> Perdidas { get; set; } = new();
}
