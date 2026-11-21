using AutoMapper;
using Granja.Domain.Entities;
using Granja.Domain.Enums;
using Granja.Services.Dtos;

namespace Granja.Services.Mappings;

public static class MapperExtensions
{
    /// <summary>Mapea un lote y calcula los datos que dependen de la fecha consultada.</summary>
    public static LoteResponse MapLote(this IMapper mapper, LoteProducto lote, DateOnly fecha)
    {
        var respuesta = mapper.Map<LoteResponse>(lote);
        respuesta.Unidad = lote.Tipo.Unidad();
        respuesta.Estado = lote.ObtenerEstado(fecha);
        respuesta.DiasParaVencer = lote.FechaVencimiento.DayNumber - fecha.DayNumber;
        return respuesta;
    }

    public static PerdidaLoteResponse ToPerdida(this LoteProducto lote) => new()
    {
        LoteId = lote.Id,
        Tipo = lote.Tipo,
        Unidad = lote.Tipo.Unidad(),
        FechaProduccion = lote.FechaProduccion,
        FechaVencimiento = lote.FechaVencimiento,
        CantidadDanada = lote.CantidadDanada
    };
}
