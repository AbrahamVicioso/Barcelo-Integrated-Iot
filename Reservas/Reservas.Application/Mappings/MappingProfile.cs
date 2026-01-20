using AutoMapper;
using Reservas.Application.DTOs;
using Reservas.Domain.Entites;

namespace Reservas.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Reserva mappings
        CreateMap<Reserva, ReservaDto>();
        CreateMap<CreateReservaDto, Reserva>()
            .ForMember(dest => dest.ReservaId, opt => opt.Ignore())
            .ForMember(dest => dest.NumeroReserva, opt => opt.Ignore())
            .ForMember(dest => dest.Estado, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.FechaActualizacion, opt => opt.Ignore())
            .ForMember(dest => dest.CheckInRealizado, opt => opt.Ignore())
            .ForMember(dest => dest.CheckOutRealizado, opt => opt.Ignore());

        CreateMap<UpdateReservaDto, Reserva>()
            .ForMember(dest => dest.HuespedId, opt => opt.Ignore())
            .ForMember(dest => dest.NumeroReserva, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.CreadoPor, opt => opt.Ignore());

        // ActividadesRecreativa mappings
        CreateMap<ActividadesRecreativa, ActividadRecreativaDto>();
        CreateMap<CreateActividadRecreativaDto, ActividadesRecreativa>()
            .ForMember(dest => dest.ActividadId, opt => opt.Ignore())
            .ForMember(dest => dest.EstaActiva, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.ReservasActividades, opt => opt.Ignore());

        CreateMap<UpdateActividadRecreativaDto, ActividadesRecreativa>()
            .ForMember(dest => dest.HotelId, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.ReservasActividades, opt => opt.Ignore());

        // ReservasActividade mappings
        CreateMap<ReservasActividade, ReservaActividadDto>();
        CreateMap<CreateReservaActividadDto, ReservasActividade>()
            .ForMember(dest => dest.ReservaActividadId, opt => opt.Ignore())
            .ForMember(dest => dest.Estado, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.RecordatorioEnviado, opt => opt.Ignore())
            .ForMember(dest => dest.FechaRecordatorio, opt => opt.Ignore())
            .ForMember(dest => dest.Actividad, opt => opt.Ignore());

        CreateMap<UpdateReservaActividadDto, ReservasActividade>()
            .ForMember(dest => dest.ActividadId, opt => opt.Ignore())
            .ForMember(dest => dest.HuespedId, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.Actividad, opt => opt.Ignore());
    }
}
