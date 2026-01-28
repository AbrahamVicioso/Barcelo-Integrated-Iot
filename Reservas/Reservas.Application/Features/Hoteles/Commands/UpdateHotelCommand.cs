using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.Hoteles.Commands;

public class UpdateHotelCommand : IRequest<Result<HotelesDto>>
{
    public int HotelId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int NumeroHabitaciones { get; set; }
    public int NumeroEstrellas { get; set; }
    public bool EstaActivo { get; set; }
}
