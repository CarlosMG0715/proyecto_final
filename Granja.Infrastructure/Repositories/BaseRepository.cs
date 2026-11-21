using Granja.Domain.Interfaces;
using Granja.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Granja.Infrastructure.Repositories;

public abstract class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly GranjaDbContext Context;

    protected BaseRepository(GranjaDbContext context)
    {
        Context = context;
    }

    protected DbSet<T> Entidades => Context.Set<T>();

    public virtual async Task<T?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        return await Entidades.FindAsync(new object[] { id }, ct);
    }

    public virtual async Task<IReadOnlyList<T>> ListarAsync(CancellationToken ct = default)
    {
        return await Entidades.ToListAsync(ct);
    }

    public virtual async Task AgregarAsync(T entidad, CancellationToken ct = default)
    {
        await Entidades.AddAsync(entidad, ct);
    }
}
