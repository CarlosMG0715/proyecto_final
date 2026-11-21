using Granja.Domain.Enums;

namespace Granja.Services.Dtos;

// ---- Ventas
public class VentaReporteItem
{
    public int VentaId { get; set; }
    public DateOnly Fecha { get; set; }
    public TipoProducto TipoProducto { get; set; }
    public string Unidad { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
}

public class ReporteVentasResponse
{
    public DateOnly? Desde { get; set; }
    public DateOnly? Hasta { get; set; }
    public TipoProducto? Tipo { get; set; }
    public List<VentaReporteItem> Ventas { get; set; } = new();
    public decimal TotalGeneral { get; set; }
}

// ---- Leche por día
public class LecheDiariaItem
{
    public DateOnly Fecha { get; set; }
    public decimal LitrosProducidos { get; set; }
    public decimal LitrosTransformados { get; set; }
    public decimal LitrosNoAprovechados { get; set; }
}

public class ReporteLecheDiariaResponse
{
    public DateOnly? Desde { get; set; }
    public DateOnly? Hasta { get; set; }
    public List<LecheDiariaItem> Dias { get; set; } = new();
    public decimal TotalProducido { get; set; }
    public decimal TotalTransformado { get; set; }
    public decimal TotalNoAprovechado { get; set; }
}

// ---- Producto por día
public class ProductoDiarioItem
{
    public DateOnly Fecha { get; set; }
    public TipoProducto Tipo { get; set; }
    public string Unidad { get; set; } = string.Empty;
    public decimal LitrosUsados { get; set; }
    public decimal CantidadElaborada { get; set; }
}

public class ReporteProductoDiarioResponse
{
    public DateOnly? Desde { get; set; }
    public DateOnly? Hasta { get; set; }
    public List<ProductoDiarioItem> Items { get; set; } = new();
}

// ---- Producto total
public class ProductoTotalItem
{
    public TipoProducto Tipo { get; set; }
    public string Unidad { get; set; } = string.Empty;
    public decimal Producido { get; set; }
    public decimal Vendido { get; set; }
    public decimal Disponible { get; set; }
    public decimal Danado { get; set; }
}

public class ReporteProductoTotalResponse
{
    public List<ProductoTotalItem> Productos { get; set; } = new();
}

// ---- Producto dañado
public class TotalDanadoItem
{
    public TipoProducto Tipo { get; set; }
    public string Unidad { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
}

public class ReporteProductoDanadoResponse
{
    public DateOnly? Desde { get; set; }
    public DateOnly? Hasta { get; set; }
    public List<PerdidaLoteResponse> Lotes { get; set; } = new();
    public List<TotalDanadoItem> Totales { get; set; } = new();
}

// ---- Vacas
public class VacaProduccionItem
{
    public int VacaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal TotalLitros { get; set; }
    public int DiasRegistrados { get; set; }
    public decimal PromedioDiario { get; set; }
}

public class ReporteVacasResponse
{
    public DateOnly? Desde { get; set; }
    public DateOnly? Hasta { get; set; }

    /// <summary>Promedio de litros producidos por vaca en el período.</summary>
    public decimal PromedioPorVaca { get; set; }
    public VacaProduccionItem? MasProductora { get; set; }
    public VacaProduccionItem? MenosProductora { get; set; }
    public List<VacaProduccionItem> BajoElPromedio { get; set; } = new();
    public List<VacaProduccionItem> Vacas { get; set; } = new();
}
