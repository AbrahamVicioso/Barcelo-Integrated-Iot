using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.ReservasActividades.Queries;

public class GetReservasActividadByUsuarioIdQueryHandler : IRequestHandler<GetReservasActividadByUsuarioIdQuery, Result<IEnumerable<ReservaActividadDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHuespedRepository _huespedRepository;
    private readonly IMapper _mapper;

    public GetReservasActividadByUsuarioIdQueryHandler(
        IUnitOfWork unitOfWork,
        IHuespedRepository huespedRepository,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _huespedRepository = huespedRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<ReservaActividadDto>>> Handle(GetReservasActividadByUsuarioIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var huespedId = await _huespedRepository.GetHuespedIdByUserIdAsync(request.UsuarioId, cancellationToken);
            if (huespedId == null)
            {
                return Result<IEnumerable<ReservaActividadDto>>.NotFound("Huésped no encontrado para este usuario.");
            }

            var reservas = await _unitOfWork.ReservasActividades.GetReservasByHuespedIdAsync(huespedId.Value, cancellationToken);
            var dtos = _mapper.Map<IEnumerable<ReservaActividadDto>>(reservas);

            return Result<IEnumerable<ReservaActividadDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ReservaActividadDto>>.Failure($"Error al obtener las reservas: {ex.Message}");
        }
    }
}