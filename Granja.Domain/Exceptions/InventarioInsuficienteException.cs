using Granja.Domain.Enums;

namespace Granja.Domain.Exceptions;

/// <summary>RN-07: no hay existencias no vencidas suficientes para la venta (409).</summary>
public class InventarioInsuficienteException : DomainException
{
    public TipoProducto Tipo { get; }
    public decimal CantidadSolicitada { get; }

    public InventarioInsuficienteException(TipoProducto tipo, decimal cantidadSolicitada)
        : base($"No hay existencias suficientes de {tipo} no vencido para despachar {cantidadSolicitada}.")
    {
        Tipo = tipo;
        CantidadSolicitada = cantidadSolicitada;
    }
}
