using Granja.Domain.Enums;
using Granja.Domain.Exceptions;

namespace Granja.Domain.Entities;

/// <summary>Venta con trazabilidad de los lotes de los que salió el producto (RN-09).</summary>
public class Venta
{
    private readonly List<VentaDetalle> _detalles = new();

    private Venta()
    {
    }

    public Venta(DateOnly fecha, TipoProducto tipo, decimal precioUnitario)
    {
        if (precioUnitario < 0)
            throw new ValidacionException("El precio unitario no puede ser negativo.");

        Fecha = fecha;
        Tipo = tipo;
        PrecioUnitario = precioUnitario;
    }

    public int Id { get; private set; }
    public DateOnly Fecha { get; private set; }
    public TipoProducto Tipo { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal Cantidad { get; private set; }
    public decimal Total { get; private set; }

    public IReadOnlyCollection<VentaDetalle> Detalles => _detalles;

    public void AgregarDetalle(LoteProducto lote, decimal cantidad)
    {
        if (cantidad <= 0)
            throw new ValidacionException("La cantidad del detalle debe ser mayor que cero.");

        _detalles.Add(new VentaDetalle(lote.Id, lote.FechaProduccion, cantidad));
        Cantidad += cantidad;
        Total = Cantidad * PrecioUnitario;
    }
}
