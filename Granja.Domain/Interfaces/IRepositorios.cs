using Granja.Domain.Entities;
using Granja.Domain.Enums;

namespace Granja.Domain.Interfaces;

public interface IGranjaRepository : IRepository<GranjaLechera>
{
    /// <summary>Devuelve la configuración de la granja con sus parámetros de producto.</summary>
    Task<GranjaLechera> ObtenerConfiguracionAsync(CancellationToken ct = default);
}

public interface IVacaRepository : IRepository<Vaca>
{
    Task<IReadOnlyList<Vaca>> ListarActivasAsync(CancellationToken ct = default);
}

public interface IProduccionRepository : IRepository<ProduccionLeche>
{
    Task<bool> ExisteProduccionAsync(DateOnly fecha, CancellationToken ct = default);
    Task AgregarRangoAsync(IEnumerable<ProduccionLeche> producciones, CancellationToken ct = default);
    Task<IReadOnlyList<ProduccionLeche>> ListarPorRangoAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
}

public interface ILoteRepository : IRepository<LoteProducto>
{
    /// <summary>Lotes con existencias y no vencidos a la fecha, ordenados por FechaProduccion ascendente (FIFO).</summary>
    Task<IReadOnlyList<LoteProducto>> ObtenerVendiblesAsync(TipoProducto tipo, DateOnly fecha, CancellationToken ct = default);

    /// <summary>Lotes ya vencidos a la fecha que todavía conservan cantidad disponible.</summary>
    Task<IReadOnlyList<LoteProducto>> ObtenerVencidosConExistenciasAsync(DateOnly fecha, CancellationToken ct = default);

    Task<IReadOnlyList<LoteProducto>> ListarPorProduccionAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
    Task<IReadOnlyList<LoteProducto>> ListarPorVencimientoAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
    Task<IReadOnlyList<LoteProducto>> ListarPorTipoAsync(TipoProducto? tipo, CancellationToken ct = default);
}

public interface IVentaRepository : IRepository<Venta>
{
    Task<Venta?> ObtenerConDetalleAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Venta>> ListarPorFiltroAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo, CancellationToken ct = default);
}
