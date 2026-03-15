using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.TiposHabitacion.Queries;

public class GetAllTiposHabitacionQueryHandler : IRequestHandler<GetAllTiposHabitacionQuery, Result<IEnumerable<TipoHabitacionDto>>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public GetAllTiposHabitacionQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<TipoHabitacionDto>>> Handle(GetAllTiposHabitacionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tipos = await _unitOfWork.TiposHabitacion.GetAll();
            var tiposDto = _mapper.Map<IEnumerable<TipoHabitacionDto>>(tipos);
            return Result<IEnumerable<TipoHabitacionDto>>.Success(tiposDto);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TipoHabitacionDto>>.Failure($"Error al obtener los tipos de habitacion: {ex.Message}");
        }
    }
}
