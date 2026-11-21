using Granja.Domain.Enums;
using Granja.Domain.Exceptions;

namespace Granja.Domain.Entities;

/// <summary>
/// Configuración de la granja: n (número de vacas), l (litros por vaca por día)
/// y los parámetros de cada producto. Ningún valor queda fijo en el código.
/// </summary>
public class GranjaLechera
{
    private readonly List<ParametroProducto> _parametros = new();

    private GranjaLechera()
    {
    }

    public GranjaLechera(string nombre, int numeroVacas, decimal litrosPorVaca)
    {
        Nombre = nombre;
        Configurar(numeroVacas, litrosPorVaca);
    }

    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;

    /// <summary>n: número de vacas de la granja.</summary>
    public int NumeroVacas { get; private set; }

    /// <summary>l: litros que produce cada vaca por día.</summary>
    public decimal LitrosPorVaca { get; private set; }

    public IReadOnlyCollection<ParametroProducto> Parametros => _parametros;

    /// <summary>Producción diaria total esperada: n × l.</summary>
    public decimal ProduccionDiariaEsperada => NumeroVacas * LitrosPorVaca;

    public void Configurar(int numeroVacas, decimal litrosPorVaca)
    {
        if (numeroVacas <= 0)
            throw new ValidacionException("El número de vacas debe ser mayor que cero.");
        if (litrosPorVaca <= 0)
            throw new ValidacionException("Los litros por vaca deben ser mayores que cero.");

        NumeroVacas = numeroVacas;
        LitrosPorVaca = litrosPorVaca;
    }

    public ParametroProducto ObtenerParametro(TipoProducto tipo) =>
        _parametros.FirstOrDefault(p => p.Tipo == tipo)
        ?? throw new EntidadNoEncontradaException($"No hay parámetros configurados para {tipo}.");

    public void ActualizarParametro(TipoProducto tipo, decimal rendimiento, int vidaUtilDias, decimal precio)
    {
        var existente = _parametros.FirstOrDefault(p => p.Tipo == tipo);
        if (existente is null)
            _parametros.Add(new ParametroProducto(Id, tipo, rendimiento, vidaUtilDias, precio));
        else
            existente.Actualizar(rendimiento, vidaUtilDias, precio);
    }
}
