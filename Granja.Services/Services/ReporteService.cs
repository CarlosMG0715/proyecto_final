using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Domain.Interfaces;
using Granja.Services.Dtos;
using Granja.Services.Mappings;

namespace Granja.Services;

public class ReporteService : IReporteService
{
    private readonly IVentaRepository _ventas;
    private readonly ILoteRepository _lotes;
    private readonly IProduccionRepository _producciones;
    private readonly IVacaRepository _vacas;

    public ReporteService(
        IVentaRepository ventas,
        ILoteRepository lotes,
        IProduccionRepository producciones,
        IVacaRepository vacas)
    {
        _ventas = ventas;
        _lotes = lotes;
        _producciones = producciones;
        _vacas = vacas;
    }

    public async Task<ReporteVentasResponse> VentasAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo, CancellationToken ct = default)
    {
        ValidarRango(desde, hasta);

        var ventas = await _ventas.ListarPorFiltroAsync(desde, hasta, tipo, ct);
        var items = ventas
            .Select(v => new VentaReporteItem
            {
                VentaId = v.Id,
                Fecha = v.Fecha,
                TipoProducto = v.Tipo,
                Unidad = v.Tipo.Unidad(),
                Cantidad = v.Cantidad,
                PrecioUnitario = v.PrecioUnitario,
                Total = v.Total
            })
            .ToList();

        return new ReporteVentasResponse
        {
            Desde = desde,
            Hasta = hasta,
            Tipo = tipo,
            Ventas = items,
            TotalGeneral = items.Sum(i => i.Total)
        };
    }

    public async Task<ReporteLecheDiariaResponse> LecheDiariaAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        ValidarRango(desde, hasta);

        var producciones = await _producciones.ListarPorRangoAsync(desde, hasta, ct);
        var lotes = await _lotes.ListarPorProduccionAsync(desde, hasta, ct);

        var producidoPorFecha = producciones
            .GroupBy(p => p.Fecha)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Litros));
        var transformadoPorFecha = lotes
            .GroupBy(l => l.FechaProduccion)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.LitrosUsados));

        var dias = producidoPorFecha.Keys
            .Union(transformadoPorFecha.Keys)
            .OrderBy(f => f)
            .Select(f =>
            {
                producidoPorFecha.TryGetValue(f, out var producido);
                transformadoPorFecha.TryGetValue(f, out var transformado);
                return new LecheDiariaItem
                {
                    Fecha = f,
                    LitrosProducidos = producido,
                    LitrosTransformados = transformado,
                    LitrosNoAprovechados = Math.Max(0, producido - transformado)
                };
            })
            .ToList();

        return new ReporteLecheDiariaResponse
        {
            Desde = desde,
            Hasta = hasta,
            Dias = dias,
            TotalProducido = dias.Sum(d => d.LitrosProducidos),
            TotalTransformado = dias.Sum(d => d.LitrosTransformados),
            TotalNoAprovechado = dias.Sum(d => d.LitrosNoAprovechados)
        };
    }

    public async Task<ReporteProductoDiarioResponse> ProductoDiarioAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        ValidarRango(desde, hasta);

        var lotes = await _lotes.ListarPorProduccionAsync(desde, hasta, ct);
        var items = lotes
            .GroupBy(l => new { l.FechaProduccion, l.Tipo })
            .OrderBy(g => g.Key.FechaProduccion)
            .ThenBy(g => g.Key.Tipo)
            .Select(g => new ProductoDiarioItem
            {
                Fecha = g.Key.FechaProduccion,
                Tipo = g.Key.Tipo,
                Unidad = g.Key.Tipo.Unidad(),
                LitrosUsados = g.Sum(l => l.LitrosUsados),
                CantidadElaborada = g.Sum(l => l.CantidadInicial)
            })
            .ToList();

        return new ReporteProductoDiarioResponse { Desde = desde, Hasta = hasta, Items = items };
    }

    public async Task<ReporteProductoTotalResponse> ProductoTotalAsync(CancellationToken ct = default)
    {
        var lotes = await _lotes.ListarPorProduccionAsync(null, null, ct);
        var ventas = await _ventas.ListarPorFiltroAsync(null, null, null, ct);

        var productos = Enum.GetValues<TipoProducto>()
            .Select(t => new ProductoTotalItem
            {
                Tipo = t,
                Unidad = t.Unidad(),
                Producido = lotes.Where(l => l.Tipo == t).Sum(l => l.CantidadInicial),
                Vendido = ventas.Where(v => v.Tipo == t).Sum(v => v.Cantidad),
                Disponible = lotes.Where(l => l.Tipo == t).Sum(l => l.CantidadDisponible),
                Danado = lotes.Where(l => l.Tipo == t).Sum(l => l.CantidadDanada)
            })
            .ToList();

        return new ReporteProductoTotalResponse { Productos = productos };
    }

    /// <summary>Producto dañado por lote y por tipo; el rango filtra por fecha de vencimiento del lote.</summary>
    public async Task<ReporteProductoDanadoResponse> ProductoDanadoAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        ValidarRango(desde, hasta);

        var lotes = (await _lotes.ListarPorVencimientoAsync(desde, hasta, ct))
            .Where(l => l.CantidadDanada > 0)
            .ToList();

        var totales = lotes
            .GroupBy(l => l.Tipo)
            .OrderBy(g => g.Key)
            .Select(g => new TotalDanadoItem
            {
                Tipo = g.Key,
                Unidad = g.Key.Unidad(),
                Cantidad = g.Sum(l => l.CantidadDanada)
            })
            .ToList();

        return new ReporteProductoDanadoResponse
        {
            Desde = desde,
            Hasta = hasta,
            Lotes = lotes.Select(l => l.ToPerdida()).ToList(),
            Totales = totales
        };
    }

    /// <summary>Producción promedio, vaca que más y que menos produce, y vacas por debajo del promedio.</summary>
    public async Task<ReporteVacasResponse> VacasAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        ValidarRango(desde, hasta);

        var vacas = await _vacas.ListarAsync(ct);
        var producciones = await _producciones.ListarPorRangoAsync(desde, hasta, ct);

        var porVaca = producciones
            .GroupBy(p => p.VacaId)
            .ToDictionary(
                g => g.Key,
                g => (Total: g.Sum(p => p.Litros), Dias: g.Select(p => p.Fecha).Distinct().Count()));

        var items = vacas
            .Where(v => v.Activa || porVaca.ContainsKey(v.Id))
            .OrderBy(v => v.Id)
            .Select(v =>
            {
                porVaca.TryGetValue(v.Id, out var datos);
                return new VacaProduccionItem
                {
                    VacaId = v.Id,
                    Nombre = v.Nombre,
                    TotalLitros = datos.Total,
                    DiasRegistrados = datos.Dias,
                    PromedioDiario = datos.Dias == 0 ? 0 : Math.Round(datos.Total / datos.Dias, 2)
                };
            })
            .ToList();

        if (items.Count == 0)
            return new ReporteVacasResponse { Desde = desde, Hasta = hasta };

        var promedio = items.Average(i => i.TotalLitros);

        return new ReporteVacasResponse
        {
            Desde = desde,
            Hasta = hasta,
            PromedioPorVaca = Math.Round(promedio, 2),
            MasProductora = items.OrderByDescending(i => i.TotalLitros).ThenBy(i => i.VacaId).First(),
            MenosProductora = items.OrderBy(i => i.TotalLitros).ThenBy(i => i.VacaId).First(),
            BajoElPromedio = items.Where(i => i.TotalLitros < promedio).ToList(),
            Vacas = items
        };
    }

    private static void ValidarRango(DateOnly? desde, DateOnly? hasta)
    {
        if (desde.HasValue && hasta.HasValue && desde > hasta)
            throw new ValidacionException("La fecha 'desde' no puede ser posterior a 'hasta'.");
    }
}
