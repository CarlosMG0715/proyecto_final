namespace Granja.Domain.Exceptions;

/// <summary>RN-02: se intenta transformar más litros de los producidos ese día (409).</summary>
public class LecheInsuficienteException : DomainException
{
    public DateOnly Fecha { get; }
    public decimal LitrosSolicitados { get; }
    public decimal LitrosDisponibles { get; }

    public LecheInsuficienteException(DateOnly fecha, decimal litrosSolicitados, decimal litrosDisponibles)
        : base($"El {fecha:yyyy-MM-dd} solo hay {litrosDisponibles} L de leche sin transformar y se solicitaron {litrosSolicitados} L.")
    {
        Fecha = fecha;
        LitrosSolicitados = litrosSolicitados;
        LitrosDisponibles = litrosDisponibles;
    }
}
