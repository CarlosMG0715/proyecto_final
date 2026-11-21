using AutoMapper;
using Granja.Domain.Entities;
using Granja.Domain.Exceptions;
using Granja.Domain.Interfaces;
using Granja.Services.Dtos;

namespace Granja.Services;

public class GranjaService : IGranjaService
{
    private readonly IGranjaRepository _granjas;
    private readonly IVacaRepository _vacas;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GranjaService(IGranjaRepository granjas, IVacaRepository vacas, IUnitOfWork uow, IMapper mapper)
    {
        _granjas = granjas;
        _vacas = vacas;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<GranjaResponse> ObtenerAsync(CancellationToken ct = default)
    {
        var granja = await _granjas.ObtenerConfiguracionAsync(ct);
        return _mapper.Map<GranjaResponse>(granja);
    }

    public async Task<GranjaResponse> ActualizarAsync(ActualizarGranjaRequest request, CancellationToken ct = default)
    {
        var granja = await _granjas.ObtenerConfiguracionAsync(ct);
        granja.Configurar(request.NumeroVacas, request.LitrosPorVaca);

        foreach (var parametro in request.Parametros ?? new List<ParametroProductoDto>())
        {
            if (!Enum.IsDefined(parametro.Tipo))
                throw new ValidacionException("Tipo de producto inválido.");

            granja.ActualizarParametro(parametro.Tipo, parametro.Rendimiento, parametro.VidaUtilDias, parametro.Precio);
        }

        await SincronizarVacasAsync(granja, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<GranjaResponse>(granja);
    }

    public async Task<IReadOnlyList<VacaResponse>> ListarVacasAsync(CancellationToken ct = default)
    {
        var vacas = await _vacas.ListarAsync(ct);
        return vacas
            .OrderBy(v => v.Id)
            .Select(v => _mapper.Map<VacaResponse>(v))
            .ToList();
    }

    /// <summary>Deja exactamente n vacas activas: reactiva o crea las que falten, o da de baja las sobrantes.</summary>
    private async Task SincronizarVacasAsync(GranjaLechera granja, CancellationToken ct)
    {
        var vacas = (await _vacas.ListarAsync(ct)).OrderBy(v => v.Id).ToList();
        var activas = vacas.Where(v => v.Activa).ToList();
        var objetivo = granja.NumeroVacas;

        if (activas.Count < objetivo)
        {
            var faltan = objetivo - activas.Count;

            var reactivar = vacas.Where(v => !v.Activa).Take(faltan).ToList();
            foreach (var vaca in reactivar)
                vaca.Activar();
            faltan -= reactivar.Count;

            var numero = vacas.Count;
            for (var i = 0; i < faltan; i++)
            {
                numero++;
                await _vacas.AgregarAsync(new Vaca(granja.Id, $"Vaca {numero:00}"), ct);
            }
        }
        else if (activas.Count > objetivo)
        {
            var sobrantes = activas.OrderByDescending(v => v.Id).Take(activas.Count - objetivo).ToList();
            foreach (var vaca in sobrantes)
                vaca.Desactivar();
        }
    }
}
