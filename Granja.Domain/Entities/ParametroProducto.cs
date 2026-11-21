using Granja.Domain.Enums;
using Granja.Domain.Exceptions;

namespace Granja.Domain.Entities;

/// <summary>Rendimiento, vida útil y precio de un producto (RN-03, RN-04).</summary>
public class ParametroProducto
{
    private ParametroProducto()
    {
    }

    public ParametroProducto(int granjaId, TipoProducto tipo, decimal rendimiento, int vidaUtilDias, decimal precio)
    {
        GranjaId = granjaId;
        Tipo = tipo;
        Actualizar(rendimiento, vidaUtilDias, precio);
    }

    public int Id { get; private set; }
    public int GranjaId { get; private set; }
    public TipoProducto Tipo { get; private set; }

    /// <summary>Litros de leche necesarios por unidad de producto (queso: 10 L = 1 kg; yogurt: 1 L = 1 L).</summary>
    public decimal Rendimiento { get; private set; }

    /// <summary>Días entre la producción y el vencimiento.</summary>
    public int VidaUtilDias { get; private set; }

    /// <summary>Precio de venta por unidad.</summary>
    public decimal Precio { get; private set; }

    public void Actualizar(decimal rendimiento, int vidaUtilDias, decimal precio)
    {
        if (rendimiento <= 0)
            throw new ValidacionException("El rendimiento debe ser mayor que cero.");
        if (vidaUtilDias <= 0)
            throw new ValidacionException("La vida útil debe ser mayor que cero.");
        if (precio < 0)
            throw new ValidacionException("El precio no puede ser negativo.");

        Rendimiento = rendimiento;
        VidaUtilDias = vidaUtilDias;
        Precio = precio;
    }
}
