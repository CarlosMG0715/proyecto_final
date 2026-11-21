namespace Granja.Domain.Exceptions;

/// <summary>RN-01: la producción de una fecha solo se registra una vez (409).</summary>
public class ProduccionYaRegistradaException : DomainException
{
    public DateOnly Fecha { get; }

    public ProduccionYaRegistradaException(DateOnly fecha)
        : base($"La producción de leche del {fecha:yyyy-MM-dd} ya fue registrada.")
    {
        Fecha = fecha;
    }
}
