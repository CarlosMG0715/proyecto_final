using Granja.Domain.Entities;
using Granja.Domain.Exceptions;
using Granja.Domain.Interfaces;
using Granja.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Granja.Infrastructure.Repositories;

public class GranjaRepository : BaseRepository<GranjaLechera>, IGranjaRepository
{
    public GranjaRepository(GranjaDbContext context) : base(context)
    {
    }

    public async Task<GranjaLechera> ObtenerConfiguracionAsync(CancellationToken ct = default)
    {
        var granja = await Context.Granjas
            .Include(g => g.Parametros)
            .OrderBy(g => g.Id)
            .FirstOrDefaultAsync(ct);

        return granja ?? throw new EntidadNoEncontradaException("No existe la configuración de la granja.");
    }
}
