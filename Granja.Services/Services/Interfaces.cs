using Granja.Domain.Enums;
using Granja.Services.Dtos;

namespace Granja.Services;

public interface IGranjaService
{
    Task<GranjaResponse> ObtenerAsync(CancellationToken ct = default);
    Task<GranjaResponse> ActualizarAsync(ActualizarGranjaRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<VacaResponse>> ListarVacasAsync(CancellationToken ct = default);
}

public interface IProduccionService
{
    Task<ProduccionDiaResponse> RegistrarProduccionAsync(RegistrarProduccionRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ProduccionDiaResponse>> ListarProduccionAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
    Task<TransformacionResponse> TransformarAsync(TransformarLecheRequest request, CancellationToken ct = default);
}

public interface IInventarioService
{
    Task<InventarioResponse> ObtenerInventarioAsync(DateOnly? fecha, TipoProducto? tipo, CancellationToken ct = default);
    Task<ProcesarVencimientosResponse> ProcesarVencimientosAsync(DateOnly? fecha, CancellationToken ct = default);
    Task<IReadOnlyList<LoteResponse>> ObtenerPorVencerAsync(int dias, DateOnly? fecha, CancellationToken ct = default);
}

public interface IVentaService
{
    Task<VentaResponse> RegistrarVentaAsync(VentaRequest request, CancellationToken ct = default);
    Task<VentaResponse> ObtenerAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<VentaResponse>> ListarAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo, CancellationToken ct = default);
}

public interface IReporteService
{
    Task<ReporteVentasResponse> VentasAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo, CancellationToken ct = default);
    Task<ReporteLecheDiariaResponse> LecheDiariaAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
    Task<ReporteProductoDiarioResponse> ProductoDiarioAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
    Task<ReporteProductoTotalResponse> ProductoTotalAsync(CancellationToken ct = default);
    Task<ReporteProductoDanadoResponse> ProductoDanadoAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
    Task<ReporteVacasResponse> VacasAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
}
