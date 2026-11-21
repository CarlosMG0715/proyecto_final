using Granja.Domain.Entities;
using Granja.Domain.Enums;
using Granja.Domain.Interfaces;
using Granja.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Granja.Infrastructure.Repositories;

public class VentaRepository : BaseRepository<Venta>, IVentaRepository
{
    public VentaRepository(GranjaDbContext context) : base(context)
    {
    }

    public async Task<Venta?> ObtenerConDetalleAsync(int id, CancellationToken ct = default)
    {
        return await Context.Ventas
            .AsNoTracking()
            .Include(v => v.Detalles)
            .FirstOrDefaultAsync(v => v.Id == id, ct);
    }

    public async Task<IReadOnlyList<Venta>> ListarPorFiltroAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo, CancellationToken ct = default)
    {
        IQueryable<Venta> consulta = Context.Ventas.AsNoTracking().Include(v => v.Detalles);

        if (desde.HasValue)
            consulta = consulta.Where(v => v.Fecha >= desde.Value);
        if (hasta.HasValue)
            consulta = consulta.Where(v => v.Fecha <= hasta.Value);
        if (tipo.HasValue)
            consulta = consulta.Where(v => v.Tipo == tipo.Value);

        return await consulta.OrderBy(v => v.Fecha).ThenBy(v => v.Id).ToListAsync(ct);
    }
}
