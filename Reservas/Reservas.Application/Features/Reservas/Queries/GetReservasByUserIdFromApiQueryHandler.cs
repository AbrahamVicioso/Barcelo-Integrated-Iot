using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.Reservas.Queries;

public class GetReservasByUserIdFromApiQueryHandler : IRequestHandler<GetReservasByUserIdFromApiQuery, Result<IEnumerable<ReservaDto>>>
{
    private readonly IUsuariosApiService _usuariosApiService;
    private readonly IReservaRepository _reservaRepository;
    private readonly IMapper _mapper;

    public GetReservasByUserIdFromApiQueryHandler(
        IUsuariosApiService usuariosApiService,
        IReservaRepository reservaRepository,
        IMapper mapper)
    {
        _usuariosApiService = usuariosApiService;
        _reservaRepository = reservaRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<ReservaDto>>> Handle(GetReservasByUserIdFromApiQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the Huesped from the Usuarios API using the UserId
            var huesped = await _usuariosApiService.GetHuespedByUsuarioIdAsync(request.UserId, cancellationToken);

            if (huesped == null)
            {
                return Result<IEnumerable<ReservaDto>>.Failure("No se encontró ningún huésped asociado a este usuario");
            }

            // Get all reservations for this huesped using the HuespedId from the API
            var reservas = await _reservaRepository.GetReservasByHuespedIdAsync(huesped.HuespedId, cancellationToken);
            var reservasDto = _mapper.Map<IEnumerable<ReservaDto>>(reservas);

            return Result<IEnumerable<ReservaDto>>.Success(reservasDto);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ReservaDto>>.Failure($"Error al obtener las reservas del huésped: {ex.Message}");
        }
    }
}
