using Granja.Domain.Entities;
using Granja.Domain.Interfaces;
using Granja.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Granja.Infrastructure.Repositories;

public class VacaRepository : BaseRepository<Vaca>, IVacaRepository
{
    public VacaRepository(GranjaDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Vaca>> ListarActivasAsync(CancellationToken ct = default)
    {
        return await Context.Vacas
            .Where(v => v.Activa)
            .OrderBy(v => v.Id)
            .ToListAsync(ct);
    }
}
