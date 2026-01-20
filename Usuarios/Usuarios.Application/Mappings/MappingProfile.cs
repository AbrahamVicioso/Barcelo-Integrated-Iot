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
        CreateMap<CreateHuespedeDto, Huespede>();
        CreateMap<UpdateHuespedeDto, Huespede>();

        // Personal Mappings
        CreateMap<Personal, PersonalDto>();
        CreateMap<CreatePersonalDto, Personal>();
        CreateMap<UpdatePersonalDto, Personal>();

        // PermisosPersonal Mappings
        CreateMap<PermisosPersonal, PermisosPersonalDto>();
        CreateMap<CreatePermisosPersonalDto, PermisosPersonal>();
        CreateMap<UpdatePermisosPersonalDto, PermisosPersonal>();
    }
}
