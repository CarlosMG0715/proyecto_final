namespace Granja.Domain.Interfaces;

/// <summary>RN-10: fecha actual inyectable para poder simular varios días en las pruebas.</summary>
public interface IClock
{
    DateOnly Today { get; }
}
