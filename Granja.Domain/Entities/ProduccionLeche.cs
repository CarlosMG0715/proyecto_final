using Granja.Domain.Exceptions;

namespace Granja.Domain.Entities;

/// <summary>Leche producida por una vaca en una fecha (RN-01: una sola vez por fecha y por vaca).</summary>
public class ProduccionLeche
{
    private ProduccionLeche()
    {
    }

    public ProduccionLeche(DateOnly fecha, int vacaId, decimal litros)
    {
        if (litros < 0)
            throw new ValidacionException("Los litros producidos no pueden ser negativos.");

        Fecha = fecha;
        VacaId = vacaId;
        Litros = litros;
    }

    public int Id { get; private set; }
    public DateOnly Fecha { get; private set; }
    public int VacaId { get; private set; }
    public decimal Litros { get; private set; }
}
