using AutoMapper;
using Reservas.Application.DTOs;
using Reservas.Application.Features.ActividadesRecreativas.Commands;
using Reservas.Application.Features.EstadosHabitacion.Commands;
using Reservas.Application.Features.TiposHabitacion.Commands;
using Reservas.Application.Features.Habitaciones.Commands;
using Reservas.Application.Features.Hoteles.Commands;
using Reservas.Application.Features.Reservas.Commands;
using Reservas.Application.Features.ReservasActividades.Commands;
using Reservas.Application.Features.EstadosReservaActividad.Commands;
using Reservas.Domain.Entites;
using Reservas.Domain.Entities;

namespace Reservas.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Reserva mappings
        CreateMap<ReservaHuesped, ReservaHuespedItemDto>();
        CreateMap<Reserva, ReservaDto>()
            .ForMember(dest => dest.EstadoNombre, opt => opt.MapFrom(src => src.EstadoReserva != null ? src.EstadoReserva.Nombre : null))
            .ForMember(dest => dest.Huespedes, opt => opt.MapFrom(src => src.ReservaHuespedes));
        CreateMap<CreateReservaCommand, Reserva>()
            .ForMember(dest => dest.EstadoReserva, opt => opt.Ignore())
            .ForMember(dest => dest.EstadoReservaId, opt => opt.Ignore())
            .ForMember(dest => dest.ReservaHuespedes, opt => opt.Ignore());
        CreateMap<UpdateReservaCommand, Reserva>()
            .ForMember(dest => dest.ReservaId, opt => opt.Ignore())
            .ForMember(dest => dest.HuespedId, opt => opt.Ignore())
            .ForMember(dest => dest.NumeroReserva, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.FechaActualizacion, opt => opt.Ignore())
            .ForMember(dest => dest.CreadoPor, opt => opt.Ignore())
            .ForMember(dest => dest.ModificadoPor, opt => opt.Ignore())
            .ForMember(dest => dest.EstadoReserva, opt => opt.Ignore())
            .ForMember(dest => dest.CheckInRealizado, opt => opt.Ignore())
            .ForMember(dest => dest.CheckOutRealizado, opt => opt.Ignore())
            .ForMember(dest => dest.ReservaHuespedes, opt => opt.Ignore());
        CreateMap<Hotel, HotelesDto>();
        CreateMap<CreateReservaDto, Reserva>()
            .ForMember(dest => dest.ReservaId, opt => opt.Ignore())
            .ForMember(dest => dest.NumeroReserva, opt => opt.Ignore())
            .ForMember(dest => dest.EstadoReservaId, opt => opt.Ignore())
            .ForMember(dest => dest.EstadoReserva, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.FechaActualizacion, opt => opt.Ignore())
            .ForMember(dest => dest.CheckInRealizado, opt => opt.Ignore())
            .ForMember(dest => dest.CheckOutRealizado, opt => opt.Ignore())
            .ForMember(dest => dest.ReservaHuespedes, opt => opt.Ignore());

        CreateMap<UpdateReservaDto, Reserva>()
            .ForMember(dest => dest.HuespedId, opt => opt.Ignore())
            .ForMember(dest => dest.NumeroReserva, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.CreadoPor, opt => opt.Ignore())
            .ForMember(dest => dest.ModificadoPor, opt => opt.Ignore())
            .ForMember(dest => dest.EstadoReserva, opt => opt.Ignore())
            .ForMember(dest => dest.CheckInRealizado, opt => opt.Ignore())
            .ForMember(dest => dest.CheckOutRealizado, opt => opt.Ignore())
            .ForMember(dest => dest.ReservaHuespedes, opt => opt.Ignore());

        // Hotel mappings
        CreateMap<CreateHotelDto, Hotel>()
            .ForMember(dest => dest.HotelId, opt => opt.Ignore())
            .ForMember(dest => dest.EstaActivo, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore());

        CreateMap<UpdateHotelCommand, Hotel>()
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore());

        // Habitacion mappings
        CreateMap<Habitacion, HabitacionDto>()
            .ForMember(dest => dest.DescripcionEstado, opt => opt.MapFrom(src => src.EstadoHabitacion != null ? src.EstadoHabitacion.Descripcion : null))
            .ForMember(dest => dest.NombreTipo, opt => opt.MapFrom(src => src.TipoHabitacion != null ? src.TipoHabitacion.Nombre : null));
        CreateMap<CreateHabitacionDto, Habitacion>()
            .ForMember(dest => dest.HabitacionId, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.EstadoHabitacion, opt => opt.Ignore())
            .ForMember(dest => dest.TipoHabitacion, opt => opt.Ignore());

        CreateMap<UpdateHabitacionCommand, Habitacion>()
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.Hotel, opt => opt.Ignore())
            .ForMember(dest => dest.EstadoHabitacion, opt => opt.Ignore())
            .ForMember(dest => dest.TipoHabitacion, opt => opt.Ignore());

        // EstadoHabitacion mappings
        CreateMap<EstadoHabitacion, EstadoHabitacionDto>();
        CreateMap<CreateEstadoHabitacionDto, EstadoHabitacion>();
        CreateMap<UpdateEstadoHabitacionCommand, EstadoHabitacion>();

        // TipoHabitacion mappings
        CreateMap<TipoHabitacion, TipoHabitacionDto>();
        CreateMap<CreateTipoHabitacionDto, TipoHabitacion>();
        CreateMap<UpdateTipoHabitacionCommand, TipoHabitacion>();
        //testing
        CreateMap<CreateActividadRecreativaCommand,ActividadesRecreativas>()
            .ForMember(dest => dest.ActividadId, opt => opt.Ignore())
            .ForMember(dest => dest.EstaActiva, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.ReservasActividades, opt => opt.Ignore());

        CreateMap<UpdateActividadRecreativaCommand, ActividadesRecreativas>()
            .ForMember(dest => dest.HotelId, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.ReservasActividades, opt => opt.Ignore());

        CreateMap<CreateReservaActividadCommand, ReservasActividades>()
            .ForMember(dest => dest.ReservaActividadId, opt => opt.Ignore())
            .ForMember(dest => dest.Estado, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.RecordatorioEnviado, opt => opt.Ignore())
            .ForMember(dest => dest.FechaRecordatorio, opt => opt.Ignore())
            .ForMember(dest => dest.Actividad, opt => opt.Ignore());

        // ActividadesRecreativa mappings
        CreateMap<ActividadesRecreativas, ActividadRecreativaDto>();
        CreateMap<CreateActividadRecreativaDto, ActividadesRecreativas>()
            .ForMember(dest => dest.ActividadId, opt => opt.Ignore())
            .ForMember(dest => dest.EstaActiva, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.ReservasActividades, opt => opt.Ignore());

        CreateMap<UpdateActividadRecreativaDto, ActividadesRecreativas>()
            .ForMember(dest => dest.HotelId, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.ReservasActividades, opt => opt.Ignore());

        // ReservasActividade mappings
        CreateMap<ReservasActividades, ReservaActividadDto>();
        CreateMap<CreateReservaActividadDto, ReservasActividades>()
            .ForMember(dest => dest.ReservaActividadId, opt => opt.Ignore())
            .ForMember(dest => dest.Estado, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.RecordatorioEnviado, opt => opt.Ignore())
            .ForMember(dest => dest.FechaRecordatorio, opt => opt.Ignore())
            .ForMember(dest => dest.Actividad, opt => opt.Ignore());

        CreateMap<UpdateReservaActividadDto, ReservasActividades>()
            .ForMember(dest => dest.ActividadId, opt => opt.Ignore())
            .ForMember(dest => dest.HuespedId, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.Actividad, opt => opt.Ignore());

        CreateMap<UpdateReservaActividadCommand, ReservasActividades>()
            .ForMember(dest => dest.ReservaActividadId, opt => opt.Ignore())
            .ForMember(dest => dest.ActividadId, opt => opt.Ignore())
            .ForMember(dest => dest.HuespedId, opt => opt.Ignore())
            .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
            .ForMember(dest => dest.EstadoReservaActividad, opt => opt.Ignore())
            .ForMember(dest => dest.Actividad, opt => opt.Ignore());

        // EstadoReservaActividad mappings
        CreateMap<EstadoReservaActividad, EstadoReservaActividadDto>();
        CreateMap<CreateEstadoReservaActividadDto, EstadoReservaActividad>()
            .ForMember(dest => dest.EstadoReservaActividadId, opt => opt.Ignore())
            .ForMember(dest => dest.ReservasActividades, opt => opt.Ignore());
        CreateMap<UpdateEstadoReservaActividadCommand, EstadoReservaActividad>()
            .ForMember(dest => dest.ReservasActividades, opt => opt.Ignore());
    }
}
