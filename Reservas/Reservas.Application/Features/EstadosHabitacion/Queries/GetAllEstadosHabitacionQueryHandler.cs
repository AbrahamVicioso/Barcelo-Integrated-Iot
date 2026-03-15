using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.EstadosHabitacion.Queries;

public class GetAllEstadosHabitacionQueryHandler : IRequestHandler<GetAllEstadosHabitacionQuery, Result<IEnumerable<EstadoHabitacionDto>>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public GetAllEstadosHabitacionQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<EstadoHabitacionDto>>> Handle(GetAllEstadosHabitacionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var estados = await _unitOfWork.EstadosHabitacion.GetAll();
            var estadosDto = _mapper.Map<IEnumerable<EstadoHabitacionDto>>(estados);
            return Result<IEnumerable<EstadoHabitacionDto>>.Success(estadosDto);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<EstadoHabitacionDto>>.Failure($"Error al obtener los estados: {ex.Message}");
        }
    }
}
