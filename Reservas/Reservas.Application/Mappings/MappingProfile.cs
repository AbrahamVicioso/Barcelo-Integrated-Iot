using AutoMapper;
using Reservas.Application.DTOs;
using Reservas.Application.Features.Habitaciones.Commands;
using Reservas.Application.Features.Hoteles.Commands;
using Reservas.Application.Features.Reservas.Commands;
using Reservas.Domain.Entites;
using Reservas.Domain.Entities;

namespace Reservas.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Reserva mappings
        CreateMap<Reserva, ReservaDto>();
        CreateMap<CreateReservaCommand,Reserva >();
        CreateMap<Hotel, HotelesDto>();
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

        // Hotel mappings
        CreateMap<CreateHotelDto, Hotel>()
            .ForMember(dest => dest.HotelId, opt => opt.Ignore())
            .ForMember(dest => dest.EstaActivo, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore());

        CreateMap<UpdateHotelCommand, Hotel>()
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore());

        // Habitacion mappings
        CreateMap<Habitacion, HabitacionDto>();
        CreateMap<CreateHabitacionDto, Habitacion>()
            .ForMember(dest => dest.HabitacionId, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore());

        CreateMap<UpdateHabitacionCommand, Habitacion>()
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.Hotel, opt => opt.Ignore());

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
