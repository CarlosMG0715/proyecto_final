using Granja.Domain.Exceptions;

namespace Granja.Domain.Entities;

public class Vaca
{
    private Vaca()
    {
    }

    public Vaca(int granjaId, string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ValidacionException("La vaca debe tener un nombre.");

        GranjaId = granjaId;
        Nombre = nombre;
        Activa = true;
    }

    public int Id { get; private set; }
    public int GranjaId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;

    /// <summary>Las vacas dadas de baja se conservan para no perder su historial de producción.</summary>
    public bool Activa { get; private set; }

    public void Activar() => Activa = true;
    public void Desactivar() => Activa = false;
}
