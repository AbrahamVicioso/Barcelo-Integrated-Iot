using AutoMapper;
using Usuarios.Application.DTOs.Departamento;
using Usuarios.Application.DTOs.Huespedes;
using Usuarios.Application.DTOs.PermisosPersonal;
using Usuarios.Application.DTOs.Personal;
using Usuarios.Application.DTOs.Puesto;
using Usuarios.Application.DTOs.TipoDocumento;
using Usuarios.Domain.Entities;

namespace Usuarios.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Huespede Mappings
        CreateMap<Huespede, HuespedeDto>()
            .ForMember(dest => dest.NombreTipoDocumento,
                opt => opt.MapFrom(src => src.TipoDocumentoNavigation != null ? src.TipoDocumentoNavigation.Nombre : string.Empty));
        CreateMap<CreateHuespedeDto, Huespede>()
            .ForMember(dest => dest.UsuarioId, opt => opt.Ignore())
            .ForMember(dest => dest.TipoDocumentoNavigation, opt => opt.Ignore());
        CreateMap<UpdateHuespedeDto, Huespede>()
            .ForMember(dest => dest.UsuarioId, opt => opt.Ignore())
            .ForMember(dest => dest.TipoDocumentoNavigation, opt => opt.Ignore());

        // Personal Mappings
        CreateMap<Personal, PersonalDto>()
            .ForMember(dest => dest.NombrePuesto,
                opt => opt.MapFrom(src => src.PuestoNavigation != null ? src.PuestoNavigation.Nombre : string.Empty))
            .ForMember(dest => dest.NombreDepartamento,
                opt => opt.MapFrom(src => src.DepartamentoNavigation != null ? src.DepartamentoNavigation.Nombre : string.Empty));
        CreateMap<CreatePersonalDto, Personal>()
            .ForMember(dest => dest.PuestoNavigation, opt => opt.Ignore())
            .ForMember(dest => dest.DepartamentoNavigation, opt => opt.Ignore());
        CreateMap<UpdatePersonalDto, Personal>()
            .ForMember(dest => dest.PuestoNavigation, opt => opt.Ignore())
            .ForMember(dest => dest.DepartamentoNavigation, opt => opt.Ignore());

        // Puesto Mappings
        CreateMap<Puesto, PuestoDto>();
        CreateMap<CreatePuestoDto, Puesto>()
            .ForMember(dest => dest.PuestoId, opt => opt.Ignore())
            .ForMember(dest => dest.EstaActivo, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.EliminadoEn, opt => opt.Ignore())
            .ForMember(dest => dest.Personals, opt => opt.Ignore());
        CreateMap<UpdatePuestoDto, Puesto>()
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.EliminadoEn, opt => opt.Ignore())
            .ForMember(dest => dest.Personals, opt => opt.Ignore());

        // Departamento Mappings
        CreateMap<Departamento, DepartamentoDto>();
        CreateMap<CreateDepartamentoDto, Departamento>()
            .ForMember(dest => dest.DepartamentoId, opt => opt.Ignore())
            .ForMember(dest => dest.EstaActivo, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.EliminadoEn, opt => opt.Ignore())
            .ForMember(dest => dest.Personals, opt => opt.Ignore());
        CreateMap<UpdateDepartamentoDto, Departamento>()
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.EliminadoEn, opt => opt.Ignore())
            .ForMember(dest => dest.Personals, opt => opt.Ignore());

        // TipoDocumento Mappings
        CreateMap<Domain.Entities.TipoDocumento, TipoDocumentoDto>();
        CreateMap<CreateTipoDocumentoDto, Domain.Entities.TipoDocumento>()
            .ForMember(dest => dest.TipoDocumentoId, opt => opt.Ignore())
            .ForMember(dest => dest.EstaActivo, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.EliminadoEn, opt => opt.Ignore())
            .ForMember(dest => dest.Huespedes, opt => opt.Ignore());
        CreateMap<UpdateTipoDocumentoDto, Domain.Entities.TipoDocumento>()
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.EliminadoEn, opt => opt.Ignore())
            .ForMember(dest => dest.Huespedes, opt => opt.Ignore());

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
