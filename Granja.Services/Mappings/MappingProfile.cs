using AutoMapper;
using Granja.Domain.Entities;
using Granja.Services.Dtos;

namespace Granja.Services.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        GranjaMapper();
        LoteMapper();
        VentaMapper();
    }

    private void GranjaMapper()
    {
        CreateMap<ParametroProducto, ParametroProductoDto>();
        CreateMap<GranjaLechera, GranjaResponse>();
        CreateMap<Vaca, VacaResponse>();
    }

    private void LoteMapper()
    {
        // Unidad, Estado y DiasParaVencer dependen de la fecha consultada: ver MapperExtensions.MapLote.
        CreateMap<LoteProducto, LoteResponse>()
            .ForMember(d => d.Unidad, o => o.Ignore())
            .ForMember(d => d.Estado, o => o.Ignore())
            .ForMember(d => d.DiasParaVencer, o => o.Ignore());
    }

    private void VentaMapper()
    {
        CreateMap<VentaDetalle, VentaDetalleResponse>();
        CreateMap<Venta, VentaResponse>()
            .ForMember(d => d.TipoProducto, o => o.MapFrom(s => s.Tipo))
            .ForMember(d => d.Detalle, o => o.MapFrom(s => s.Detalles));
    }
}
