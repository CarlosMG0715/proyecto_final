namespace Granja.Domain.Interfaces;

/// <summary>Confirma en una sola transacción todos los cambios hechos por los repositorios.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
