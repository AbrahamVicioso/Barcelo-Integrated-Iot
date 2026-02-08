using AutoMapper;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Features.Dispositivos.Commands;
using Dispositivos.Domain.Entities;

namespace Dispositivos.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Dispositivo mappings
        CreateMap<Dispositivo, DispositivoDto>();
        
        CreateMap<CreateDispositivoDto, Dispositivo>()
            .ForMember(dest => dest.DispositivoId, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore());
        
        CreateMap<UpdateDispositivoDto, Dispositivo>()
            .ForMember(dest => dest.DispositivoId, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.CerradurasInteligentes, opt => opt.Ignore())
            .ForMember(dest => dest.MantenimientoCerraduras, opt => opt.Ignore());
    }
}
