using Granja.Domain.Entities;
using Granja.Domain.Interfaces;
using Granja.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Granja.Infrastructure.Repositories;

public class ProduccionRepository : BaseRepository<ProduccionLeche>, IProduccionRepository
{
    public ProduccionRepository(GranjaDbContext context) : base(context)
    {
    }

    public async Task<bool> ExisteProduccionAsync(DateOnly fecha, CancellationToken ct = default)
    {
        return await Context.Producciones.AnyAsync(p => p.Fecha == fecha, ct);
    }

    public async Task AgregarRangoAsync(IEnumerable<ProduccionLeche> producciones, CancellationToken ct = default)
    {
        await Context.Producciones.AddRangeAsync(producciones, ct);
    }

    public async Task<IReadOnlyList<ProduccionLeche>> ListarPorRangoAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        IQueryable<ProduccionLeche> consulta = Context.Producciones.AsNoTracking();

        if (desde.HasValue)
            consulta = consulta.Where(p => p.Fecha >= desde.Value);
        if (hasta.HasValue)
            consulta = consulta.Where(p => p.Fecha <= hasta.Value);

        return await consulta.OrderBy(p => p.Fecha).ThenBy(p => p.VacaId).ToListAsync(ct);
    }
}
