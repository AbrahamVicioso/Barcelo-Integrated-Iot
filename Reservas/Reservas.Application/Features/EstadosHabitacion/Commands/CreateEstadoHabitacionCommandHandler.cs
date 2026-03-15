using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entities;

namespace Reservas.Application.Features.EstadosHabitacion.Commands;

public class CreateEstadoHabitacionCommandHandler : IRequestHandler<CreateEstadoHabitacionCommand, Result<int>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEstadoHabitacionCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(CreateEstadoHabitacionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var estado = _mapper.Map<EstadoHabitacion>(request.EstadoHabitacion);
            await _unitOfWork.EstadosHabitacion.AddAsync(estado, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<int>.Success(estado.EstadoHabitacionId);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error al crear el estado: {ex.Message}");
        }
    }
}
