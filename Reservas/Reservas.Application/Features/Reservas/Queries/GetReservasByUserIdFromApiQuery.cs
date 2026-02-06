using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.Reservas.Queries;

public class GetReservasByUserIdFromApiQuery : IRequest<Result<IEnumerable<ReservaDto>>>
{
    public string UserId { get; set; } = string.Empty;
}
