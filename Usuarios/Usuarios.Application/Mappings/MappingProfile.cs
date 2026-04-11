using AutoMapper;
using Usuarios.Application.DTOs.Huespedes;
using Usuarios.Application.DTOs.PermisosPersonal;
using Usuarios.Application.DTOs.Personal;
using Usuarios.Domain.Entities;

namespace Usuarios.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Huespede Mappings
        CreateMap<Huespede, HuespedeDto>();
        CreateMap<CreateHuespedeDto, Huespede>()
            .ForMember(dest => dest.UsuarioId, opt => opt.Ignore());
        CreateMap<UpdateHuespedeDto, Huespede>()
            .ForMember(dest => dest.UsuarioId, opt => opt.Ignore());

        // Personal Mappings
        CreateMap<Personal, PersonalDto>();
        CreateMap<CreatePersonalDto, Personal>();
        CreateMap<UpdatePersonalDto, Personal>();

        // PermisosPersonal Mappings
        CreateMap<PermisosPersonal, PermisosPersonalDto>()
            .ForMember(dest => dest.NombrePersonal,
                opt => opt.MapFrom(src => src.Personal != null ? src.Personal.NombreCompleto : null))
            .ForMember(dest => dest.NombreHabitacion,
                opt => opt.MapFrom(src => src.Habitacion != null ? src.Habitacion.NumeroHabitacion : null))
            .ForMember(dest => dest.NombreActividad,
                opt => opt.MapFrom(src => src.Actividad != null ? src.Actividad.NombreActividad : null))
            .ForMember(dest => dest.OtorgadoPorNombre,
                opt => opt.MapFrom(src => src.OtorgadoPorNavigation != null
                    ? (src.OtorgadoPorNavigation.UserName ?? src.OtorgadoPorNavigation.Email)
                    : null));
        CreateMap<CreatePermisosPersonalDto, PermisosPersonal>();
        CreateMap<UpdatePermisosPersonalDto, PermisosPersonal>();
    }
}
