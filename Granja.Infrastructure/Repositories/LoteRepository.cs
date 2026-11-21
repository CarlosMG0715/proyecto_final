using Granja.Domain.Entities;
using Granja.Domain.Enums;
using Granja.Domain.Interfaces;
using Granja.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Granja.Infrastructure.Repositories;

/// <remarks>
/// SQLite guarda los decimal como texto y no puede compararlos ni sumarlos en SQL.
/// Por eso los filtros sobre cantidades se aplican en memoria, después de filtrar por tipo y fechas en la base.
/// </remarks>
public class LoteRepository : BaseRepository<LoteProducto>, ILoteRepository
{
    public LoteRepository(GranjaDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<LoteProducto>> ObtenerVendiblesAsync(TipoProducto tipo, DateOnly fecha, CancellationToken ct = default)
    {
        var candidatos = await Context.Lotes
            .Where(l => l.Tipo == tipo && l.FechaProduccion <= fecha && l.FechaVencimiento >= fecha)
            .ToListAsync(ct);

        // FIFO: el más antiguo primero (por fecha de producción; el Id desempata).
        return candidatos
            .Where(l => l.EsVendible(fecha))
            .OrderBy(l => l.FechaProduccion)
            .ThenBy(l => l.Id)
            .ToList();
    }

    public async Task<IReadOnlyList<LoteProducto>> ObtenerVencidosConExistenciasAsync(DateOnly fecha, CancellationToken ct = default)
    {
        var vencidos = await Context.Lotes
            .Where(l => l.FechaVencimiento < fecha)
            .ToListAsync(ct);

        return vencidos
            .Where(l => l.CantidadDisponible > 0)
            .OrderBy(l => l.FechaProduccion)
            .ThenBy(l => l.Id)
            .ToList();
    }

    public async Task<IReadOnlyList<LoteProducto>> ListarPorProduccionAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        IQueryable<LoteProducto> consulta = Context.Lotes.AsNoTracking();

        if (desde.HasValue)
            consulta = consulta.Where(l => l.FechaProduccion >= desde.Value);
        if (hasta.HasValue)
            consulta = consulta.Where(l => l.FechaProduccion <= hasta.Value);

        return await consulta.OrderBy(l => l.FechaProduccion).ThenBy(l => l.Id).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LoteProducto>> ListarPorVencimientoAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        IQueryable<LoteProducto> consulta = Context.Lotes.AsNoTracking();

        if (desde.HasValue)
            consulta = consulta.Where(l => l.FechaVencimiento >= desde.Value);
        if (hasta.HasValue)
            consulta = consulta.Where(l => l.FechaVencimiento <= hasta.Value);

        return await consulta.OrderBy(l => l.FechaVencimiento).ThenBy(l => l.Id).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LoteProducto>> ListarPorTipoAsync(TipoProducto? tipo, CancellationToken ct = default)
    {
        IQueryable<LoteProducto> consulta = Context.Lotes.AsNoTracking();

        if (tipo.HasValue)
            consulta = consulta.Where(l => l.Tipo == tipo.Value);

        return await consulta.OrderBy(l => l.FechaProduccion).ThenBy(l => l.Id).ToListAsync(ct);
    }
}
