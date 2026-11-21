namespace Granja.Domain.Exceptions;

/// <summary>Base de las excepciones de negocio. La capa Web decide el código HTTP.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}

/// <summary>Datos de entrada inválidos o regla violada por el usuario (400).</summary>
public class ValidacionException : DomainException
{
    public ValidacionException(string message) : base(message)
    {
    }
}

/// <summary>El recurso solicitado no existe (404).</summary>
public class EntidadNoEncontradaException : DomainException
{
    public EntidadNoEncontradaException(string message) : base(message)
    {
    }
}
