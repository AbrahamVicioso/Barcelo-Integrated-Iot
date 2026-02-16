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

        // CerradurasInteligente mappings
        CreateMap<CerradurasInteligente, CerradurasInteligenteDto>();
        
        CreateMap<CreateCerradurasInteligenteDto, CerradurasInteligente>();
        
        CreateMap<UpdateCerradurasInteligenteDto, CerradurasInteligente>();

        // CredencialesAcceso mappings
        CreateMap<CredencialesAcceso, CredencialesAccesoDto>();
        
        CreateMap<CreateCredencialesAccesoDto, CredencialesAcceso>()
            .ForMember(dest => dest.CredencialId, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.HashPin, opt => opt.Ignore())
            .ForMember(dest => dest.UltimoUso, opt => opt.Ignore());
        
        CreateMap<UpdateCredencialesAccesoDto, CredencialesAcceso>();

        // MantenimientoCerradura mappings
        CreateMap<MantenimientoCerradura, MantenimientoCerraduraDto>();
        
        CreateMap<CreateMantenimientoCerraduraDto, MantenimientoCerradura>()
            .ForMember(dest => dest.MantenimientoId, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore());
        
        CreateMap<UpdateMantenimientoCerraduraDto, MantenimientoCerradura>();

        // RegistrosAcceso mappings
        CreateMap<RegistrosAcceso, RegistrosAccesoDto>();
        
        CreateMap<CreateRegistrosAccesoDto, RegistrosAcceso>()
            .ForMember(dest => dest.RegistroId, opt => opt.Ignore());

        // RegistrosAuditorium mappings
        CreateMap<RegistrosAuditorium, RegistrosAuditoriumDto>();
        
        CreateMap<RegistrosAuditoriumDto, RegistrosAuditorium>()
            .ForMember(dest => dest.AuditoriaId, opt => opt.Ignore());
    }
}
