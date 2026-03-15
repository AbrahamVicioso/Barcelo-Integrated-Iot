using AutoMapper;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Features.Dispositivos.Commands;
using Dispositivos.Application.Features.EstadosDispositivo.Commands;
using Dispositivos.Application.Features.TiposDispositivo.Commands;
using Dispositivos.Domain.Entities;

namespace Dispositivos.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Dispositivo mappings
        CreateMap<Dispositivo, DispositivoDto>()
            .ForMember(dest => dest.DescripcionEstado, opt => opt.MapFrom(src => src.EstadoDispositivo != null ? src.EstadoDispositivo.Descripcion : null))
            .ForMember(dest => dest.NombreTipo,        opt => opt.MapFrom(src => src.TipoDispositivo   != null ? src.TipoDispositivo.Nombre         : null));

        CreateMap<CreateDispositivoDto, Dispositivo>()
            .ForMember(dest => dest.DispositivoId,   opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion,   opt => opt.Ignore())
            .ForMember(dest => dest.EstadoDispositivo, opt => opt.Ignore())
            .ForMember(dest => dest.TipoDispositivo,   opt => opt.Ignore());

        CreateMap<UpdateDispositivoDto, Dispositivo>()
            .ForMember(dest => dest.DispositivoId,          opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion,          opt => opt.Ignore())
            .ForMember(dest => dest.CerradurasInteligentes, opt => opt.Ignore())
            .ForMember(dest => dest.MantenimientoCerraduras, opt => opt.Ignore())
            .ForMember(dest => dest.EstadoDispositivo,       opt => opt.Ignore())
            .ForMember(dest => dest.TipoDispositivo,         opt => opt.Ignore());

        // TipoDispositivo mappings
        CreateMap<TipoDispositivo, TipoDispositivoDto>();
        CreateMap<CreateTipoDispositivoDto, TipoDispositivo>();
        CreateMap<UpdateTipoDispositivoCommand, TipoDispositivo>();

        // EstadoDispositivo mappings
        CreateMap<EstadoDispositivo, EstadoDispositivoDto>();
        CreateMap<CreateEstadoDispositivoDto, EstadoDispositivo>();
        CreateMap<UpdateEstadoDispositivoCommand, EstadoDispositivo>();

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
