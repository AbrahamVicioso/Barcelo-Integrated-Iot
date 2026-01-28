using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.Hoteles.Commands;

public class CreateHotelCommand : IRequest<Result<int>>
{
    public CreateHotelDto Hotel { get; set; } = new();
}