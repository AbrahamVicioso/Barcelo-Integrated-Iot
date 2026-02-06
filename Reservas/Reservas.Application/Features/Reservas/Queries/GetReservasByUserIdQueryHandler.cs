using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.Reservas.Queries;

public class GetReservasByUserIdQueryHandler : IRequestHandler<GetReservasByUserIdQuery, Result<IEnumerable<ReservaDto>>>
{
    private readonly IHuespedRepository _huespedRepository;
    private readonly IReservaRepository _reservaRepository;
    private readonly IMapper _mapper;

    public GetReservasByUserIdQueryHandler(
        IHuespedRepository huespedRepository,
        IReservaRepository reservaRepository,
        IMapper mapper)
    {
        _huespedRepository = huespedRepository;
        _reservaRepository = reservaRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<ReservaDto>>> Handle(GetReservasByUserIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the HuespedId by UserId
            var huespedId = await _huespedRepository.GetHuespedIdByUserIdAsync(request.UserId, cancellationToken);

            if (huespedId == null)
            {
                return Result<IEnumerable<ReservaDto>>.Failure("No se encontró ningún huésped asociado a este usuario");
            }

            // Get all reservations for this huesped
            var reservas = await _reservaRepository.GetReservasByHuespedIdAsync(huespedId.Value, cancellationToken);
            var reservasDto = _mapper.Map<IEnumerable<ReservaDto>>(reservas);

            return Result<IEnumerable<ReservaDto>>.Success(reservasDto);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ReservaDto>>.Failure($"Error al obtener las reservas del huésped: {ex.Message}");
        }
    }
}
