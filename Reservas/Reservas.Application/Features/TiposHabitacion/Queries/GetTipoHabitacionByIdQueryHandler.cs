using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.TiposHabitacion.Queries;

public class GetTipoHabitacionByIdQueryHandler : IRequestHandler<GetTipoHabitacionByIdQuery, Result<TipoHabitacionDto>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public GetTipoHabitacionByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TipoHabitacionDto>> Handle(GetTipoHabitacionByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tipo = await _unitOfWork.TiposHabitacion.GetById(request.TipoHabitacionId);
            if (tipo == null)
                return Result<TipoHabitacionDto>.Failure($"Tipo de habitacion con ID {request.TipoHabitacionId} no encontrado.");

            return Result<TipoHabitacionDto>.Success(_mapper.Map<TipoHabitacionDto>(tipo));
        }
        catch (Exception ex)
        {
            return Result<TipoHabitacionDto>.Failure($"Error al obtener el tipo de habitacion: {ex.Message}");
        }
    }
}
