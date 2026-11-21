using Granja.Domain.Enums;
using Granja.Domain.Exceptions;

namespace Granja.Domain.Entities;

/// <summary>
/// Lote de producto terminado (RN-04). Las reglas de vencimiento y descuento viven aquí, no en el servicio.
/// </summary>
public class LoteProducto
{
    private LoteProducto()
    {
    }

    /// <summary>Crea el lote resultante de transformar <paramref name="litrosUsados"/> litros de leche (RN-03, RN-04).</summary>
    public LoteProducto(TipoProducto tipo, DateOnly fechaProduccion, decimal litrosUsados, decimal rendimiento, int vidaUtilDias)
    {
        if (litrosUsados <= 0)
            throw new ValidacionException("Los litros a transformar deben ser mayores que cero.");
        if (rendimiento <= 0)
            throw new ValidacionException("El rendimiento debe ser mayor que cero.");
        if (vidaUtilDias <= 0)
            throw new ValidacionException("La vida útil debe ser mayor que cero.");

        Tipo = tipo;
        FechaProduccion = fechaProduccion;
        FechaVencimiento = fechaProduccion.AddDays(vidaUtilDias);
        LitrosUsados = litrosUsados;
        CantidadInicial = Math.Round(litrosUsados / rendimiento, 4, MidpointRounding.AwayFromZero);
        CantidadDisponible = CantidadInicial;
    }

    public int Id { get; private set; }
    public TipoProducto Tipo { get; private set; }
    public DateOnly FechaProduccion { get; private set; }
    public DateOnly FechaVencimiento { get; private set; }

    /// <summary>Litros de leche consumidos por la transformación que originó este lote.</summary>
    public decimal LitrosUsados { get; private set; }
    public decimal CantidadInicial { get; private set; }
    public decimal CantidadDisponible { get; private set; }
    public decimal CantidadDanada { get; private set; }

    /// <summary>RN-08: un lote está vencido cuando la fecha consultada es posterior a su vencimiento.</summary>
    public bool EstaVencido(DateOnly fecha) => fecha > FechaVencimiento;

    /// <summary>Un lote se puede vender si ya existe a esa fecha, tiene existencias y no está vencido.</summary>
    public bool EsVendible(DateOnly fecha) =>
        FechaProduccion <= fecha && CantidadDisponible > 0 && !EstaVencido(fecha);

    public void Descontar(decimal cantidad)
    {
        if (cantidad <= 0 || cantidad > CantidadDisponible)
            throw new ValidacionException("Cantidad inválida para el lote.");

        CantidadDisponible -= cantidad;
    }

    /// <summary>RN-08: la cantidad disponible pasa a dañada y el lote deja de ser vendible.</summary>
    public decimal MarcarComoDanado()
    {
        var perdida = CantidadDisponible;
        CantidadDanada += perdida;
        CantidadDisponible = 0;
        return perdida;
    }

    public EstadoLote ObtenerEstado(DateOnly fecha)
    {
        if (CantidadDanada > 0 || (CantidadDisponible > 0 && EstaVencido(fecha)))
            return EstadoLote.Vencido;

        return CantidadDisponible > 0 ? EstadoLote.Vigente : EstadoLote.Agotado;
    }
}
