using System.ComponentModel.DataAnnotations;
using Granja.Domain.Enums;

namespace Granja.Services.Dtos;

public class VentaRequest
{
    [EnumDataType(typeof(TipoProducto))]
    public TipoProducto TipoProducto { get; set; }

    [Range(0.0001, 10000000)]
    public decimal Cantidad { get; set; }

    /// <summary>Fecha de la venta (yyyy-MM-dd). Por defecto, la fecha actual del IClock (RN-10).</summary>
    public DateOnly? Fecha { get; set; }
}

public class VentaDetalleResponse
{
    public int LoteId { get; set; }
    public DateOnly FechaProduccion { get; set; }
    public decimal Cantidad { get; set; }
}

public class VentaResponse
{
    public int Id { get; set; }
    public DateOnly Fecha { get; set; }
    public TipoProducto TipoProducto { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
    public List<VentaDetalleResponse> Detalle { get; set; } = new();
}
