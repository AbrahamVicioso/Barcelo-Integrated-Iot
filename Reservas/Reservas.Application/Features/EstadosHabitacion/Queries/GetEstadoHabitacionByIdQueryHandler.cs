using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.EstadosHabitacion.Queries;

public class GetEstadoHabitacionByIdQueryHandler : IRequestHandler<GetEstadoHabitacionByIdQuery, Result<EstadoHabitacionDto>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public GetEstadoHabitacionByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<EstadoHabitacionDto>> Handle(GetEstadoHabitacionByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var estado = await _unitOfWork.EstadosHabitacion.GetById(request.EstadoHabitacionId);
            if (estado == null)
                return Result<EstadoHabitacionDto>.Failure($"Estado con ID {request.EstadoHabitacionId} no encontrado.");

            return Result<EstadoHabitacionDto>.Success(_mapper.Map<EstadoHabitacionDto>(estado));
        }
        catch (Exception ex)
        {
            return Result<EstadoHabitacionDto>.Failure($"Error al obtener el estado: {ex.Message}");
        }
    }
}
