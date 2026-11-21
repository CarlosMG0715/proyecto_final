using Granja.Domain.Interfaces;

namespace Granja.Infrastructure.Data;

/// <summary>
/// SaveChanges de EF Core ejecuta todos los cambios pendientes en una sola transacción:
/// la venta y el descuento de los lotes se confirman juntos o no se confirman.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly GranjaDbContext _context;

    public UnitOfWork(GranjaDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
