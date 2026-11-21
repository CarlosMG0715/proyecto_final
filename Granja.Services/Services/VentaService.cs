using AutoMapper;
using Granja.Domain.Entities;
using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Domain.Interfaces;
using Granja.Services.Dtos;

namespace Granja.Services;

public class VentaService : IVentaService
{
    private readonly IGranjaRepository _granjas;
    private readonly ILoteRepository _lotes;
    private readonly IVentaRepository _ventas;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly IMapper _mapper;

    public VentaService(
        IGranjaRepository granjas,
        ILoteRepository lotes,
        IVentaRepository ventas,
        IUnitOfWork uow,
        IClock clock,
        IMapper mapper)
    {
        _granjas = granjas;
        _lotes = lotes;
        _ventas = ventas;
        _uow = uow;
        _clock = clock;
        _mapper = mapper;
    }

    /// <summary>
    /// RN-05, RN-06, RN-07, RN-09: venta ad libitum con FIFO. Es atómica: o se despacha todo o no cambia nada.
    /// </summary>
    public async Task<VentaResponse> RegistrarVentaAsync(VentaRequest request, CancellationToken ct = default)
    {
        if (!Enum.IsDefined(request.TipoProducto))
            throw new ValidacionException("Tipo de producto inválido.");
        if (request.Cantidad <= 0)
            throw new ValidacionException("La cantidad debe ser mayor que cero.");

        var fecha = request.Fecha ?? _clock.Today;

        var granja = await _granjas.ObtenerConfiguracionAsync(ct);
        var precio = granja.ObtenerParametro(request.TipoProducto).Precio;

        var lotes = await _lotes.ObtenerVendiblesAsync(request.TipoProducto, fecha, ct); // FechaProduccion ASC

        if (lotes.Sum(l => l.CantidadDisponible) < request.Cantidad)
            throw new InventarioInsuficienteException(request.TipoProducto, request.Cantidad);

        var venta = new Venta(fecha, request.TipoProducto, precio);
        var restante = request.Cantidad;

        foreach (var lote in lotes)
        {
            if (restante <= 0)
                break;

            var tomar = Math.Min(lote.CantidadDisponible, restante);
            lote.Descontar(tomar);
            venta.AgregarDetalle(lote, tomar);
            restante -= tomar;
        }

        await _ventas.AgregarAsync(venta, ct);
        await _uow.SaveChangesAsync(ct); // una sola transacción: venta + descuento de lotes

        return _mapper.Map<VentaResponse>(venta);
    }

    public async Task<VentaResponse> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var venta = await _ventas.ObtenerConDetalleAsync(id, ct)
            ?? throw new EntidadNoEncontradaException($"La venta {id} no existe.");

        return _mapper.Map<VentaResponse>(venta);
    }

    public async Task<IReadOnlyList<VentaResponse>> ListarAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo, CancellationToken ct = default)
    {
        if (desde.HasValue && hasta.HasValue && desde > hasta)
            throw new ValidacionException("La fecha 'desde' no puede ser posterior a 'hasta'.");

        var ventas = await _ventas.ListarPorFiltroAsync(desde, hasta, tipo, ct);
        return ventas.Select(v => _mapper.Map<VentaResponse>(v)).ToList();
    }
}
