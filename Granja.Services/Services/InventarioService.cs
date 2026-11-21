using AutoMapper;
using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Domain.Interfaces;
using Granja.Services.Dtos;
using Granja.Services.Mappings;

namespace Granja.Services;

public class InventarioService : IInventarioService
{
    private readonly ILoteRepository _lotes;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly IMapper _mapper;

    public InventarioService(ILoteRepository lotes, IUnitOfWork uow, IClock clock, IMapper mapper)
    {
        _lotes = lotes;
        _uow = uow;
        _clock = clock;
        _mapper = mapper;
    }

    public async Task<InventarioResponse> ObtenerInventarioAsync(DateOnly? fecha, TipoProducto? tipo, CancellationToken ct = default)
    {
        var dia = fecha ?? _clock.Today;
        var lotes = await _lotes.ListarPorTipoAsync(tipo, ct);

        var resumen = Enum.GetValues<TipoProducto>()
            .Where(t => tipo is null || t == tipo)
            .Select(t =>
            {
                var delTipo = lotes.Where(l => l.Tipo == t).ToList();
                return new ResumenInventarioItem
                {
                    Tipo = t,
                    Unidad = t.Unidad(),
                    Vendible = delTipo.Where(l => !l.EstaVencido(dia)).Sum(l => l.CantidadDisponible),
                    VencidoSinProcesar = delTipo.Where(l => l.EstaVencido(dia)).Sum(l => l.CantidadDisponible),
                    Danado = delTipo.Sum(l => l.CantidadDanada)
                };
            })
            .ToList();

        return new InventarioResponse
        {
            Fecha = dia,
            Resumen = resumen,
            Lotes = lotes.Select(l => _mapper.MapLote(l, dia)).ToList()
        };
    }

    /// <summary>RN-08: la cantidad disponible de los lotes vencidos pasa a dañada.</summary>
    public async Task<ProcesarVencimientosResponse> ProcesarVencimientosAsync(DateOnly? fecha, CancellationToken ct = default)
    {
        var dia = fecha ?? _clock.Today;
        var vencidos = await _lotes.ObtenerVencidosConExistenciasAsync(dia, ct);

        foreach (var lote in vencidos)
            lote.MarcarComoDanado();

        await _uow.SaveChangesAsync(ct);

        return new ProcesarVencimientosResponse
        {
            Fecha = dia,
            LotesProcesados = vencidos.Count,
            Perdidas = vencidos.Select(l => l.ToPerdida()).ToList()
        };
    }

    /// <summary>Extensión opcional: lotes con existencias que vencen en los próximos <paramref name="dias"/> días.</summary>
    public async Task<IReadOnlyList<LoteResponse>> ObtenerPorVencerAsync(int dias, DateOnly? fecha, CancellationToken ct = default)
    {
        if (dias < 0)
            throw new ValidacionException("Los días deben ser mayores o iguales a cero.");

        var dia = fecha ?? _clock.Today;
        var limite = dia.AddDays(dias);
        var lotes = await _lotes.ListarPorTipoAsync(null, ct);

        return lotes
            .Where(l => l.CantidadDisponible > 0 && !l.EstaVencido(dia) && l.FechaVencimiento <= limite)
            .OrderBy(l => l.FechaVencimiento)
            .Select(l => _mapper.MapLote(l, dia))
            .ToList();
    }
}
