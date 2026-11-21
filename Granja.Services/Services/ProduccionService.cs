using AutoMapper;
using Granja.Domain.Entities;
using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Domain.Interfaces;
using Granja.Services.Dtos;
using Granja.Services.Mappings;

namespace Granja.Services;

public class ProduccionService : IProduccionService
{
    private readonly IGranjaRepository _granjas;
    private readonly IVacaRepository _vacas;
    private readonly IProduccionRepository _producciones;
    private readonly ILoteRepository _lotes;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly IMapper _mapper;

    public ProduccionService(
        IGranjaRepository granjas,
        IVacaRepository vacas,
        IProduccionRepository producciones,
        ILoteRepository lotes,
        IUnitOfWork uow,
        IClock clock,
        IMapper mapper)
    {
        _granjas = granjas;
        _vacas = vacas;
        _producciones = producciones;
        _lotes = lotes;
        _uow = uow;
        _clock = clock;
        _mapper = mapper;
    }

    /// <summary>RN-01: cada vaca produce l litros; la producción de una fecha se registra una sola vez.</summary>
    public async Task<ProduccionDiaResponse> RegistrarProduccionAsync(RegistrarProduccionRequest request, CancellationToken ct = default)
    {
        var fecha = request.Fecha ?? _clock.Today;

        if (await _producciones.ExisteProduccionAsync(fecha, ct))
            throw new ProduccionYaRegistradaException(fecha);

        var granja = await _granjas.ObtenerConfiguracionAsync(ct);
        var vacas = await _vacas.ListarActivasAsync(ct);
        if (vacas.Count == 0)
            throw new ValidacionException("La granja no tiene vacas activas.");

        var especificos = new Dictionary<int, decimal>();
        foreach (var item in request.Vacas ?? new List<ProduccionVacaRequest>())
        {
            if (item.Litros < 0)
                throw new ValidacionException("Los litros de una vaca no pueden ser negativos.");
            if (vacas.All(v => v.Id != item.VacaId))
                throw new ValidacionException($"La vaca {item.VacaId} no existe o no está activa.");
            if (!especificos.TryAdd(item.VacaId, item.Litros))
                throw new ValidacionException($"La vaca {item.VacaId} aparece repetida.");
        }

        var registros = vacas
            .Select(v => new ProduccionLeche(
                fecha,
                v.Id,
                especificos.TryGetValue(v.Id, out var litros) ? litros : granja.LitrosPorVaca))
            .ToList();

        await _producciones.AgregarRangoAsync(registros, ct);
        await _uow.SaveChangesAsync(ct);

        return ConstruirDia(fecha, registros, vacas);
    }

    public async Task<IReadOnlyList<ProduccionDiaResponse>> ListarProduccionAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        if (desde.HasValue && hasta.HasValue && desde > hasta)
            throw new ValidacionException("La fecha 'desde' no puede ser posterior a 'hasta'.");

        var registros = await _producciones.ListarPorRangoAsync(desde, hasta, ct);
        var vacas = await _vacas.ListarAsync(ct);

        return registros
            .GroupBy(r => r.Fecha)
            .OrderBy(g => g.Key)
            .Select(g => ConstruirDia(g.Key, g.ToList(), vacas))
            .ToList();
    }

    /// <summary>RN-02, RN-03, RN-04: transforma leche de una fecha en un lote de queso o yogurt.</summary>
    public async Task<TransformacionResponse> TransformarAsync(TransformarLecheRequest request, CancellationToken ct = default)
    {
        if (!Enum.IsDefined(request.TipoProducto))
            throw new ValidacionException("Tipo de producto inválido.");
        if (request.Litros <= 0)
            throw new ValidacionException("Los litros a transformar deben ser mayores que cero.");

        var fecha = request.Fecha ?? _clock.Today;

        var producidos = (await _producciones.ListarPorRangoAsync(fecha, fecha, ct)).Sum(p => p.Litros);
        var transformados = (await _lotes.ListarPorProduccionAsync(fecha, fecha, ct)).Sum(l => l.LitrosUsados);
        var disponibles = producidos - transformados;

        if (request.Litros > disponibles)
            throw new LecheInsuficienteException(fecha, request.Litros, disponibles);

        var granja = await _granjas.ObtenerConfiguracionAsync(ct);
        var parametro = granja.ObtenerParametro(request.TipoProducto);

        var lote = new LoteProducto(request.TipoProducto, fecha, request.Litros, parametro.Rendimiento, parametro.VidaUtilDias);
        await _lotes.AgregarAsync(lote, ct);
        await _uow.SaveChangesAsync(ct);

        return new TransformacionResponse
        {
            Lote = _mapper.MapLote(lote, fecha),
            LitrosDisponiblesRestantes = disponibles - request.Litros
        };
    }

    private static ProduccionDiaResponse ConstruirDia(DateOnly fecha, IReadOnlyList<ProduccionLeche> registros, IReadOnlyList<Vaca> vacas)
    {
        var nombres = vacas.ToDictionary(v => v.Id, v => v.Nombre);

        return new ProduccionDiaResponse
        {
            Fecha = fecha,
            NumeroVacas = registros.Count,
            TotalLitros = registros.Sum(r => r.Litros),
            Detalle = registros
                .Select(r => new ProduccionVacaResponse
                {
                    VacaId = r.VacaId,
                    Vaca = nombres.TryGetValue(r.VacaId, out var nombre) ? nombre : $"Vaca {r.VacaId}",
                    Litros = r.Litros
                })
                .ToList()
        };
    }
}
