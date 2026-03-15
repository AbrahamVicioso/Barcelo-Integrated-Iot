using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entities;

namespace Reservas.Application.Features.TiposHabitacion.Commands;

public class CreateTipoHabitacionCommandHandler : IRequestHandler<CreateTipoHabitacionCommand, Result<int>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTipoHabitacionCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(CreateTipoHabitacionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tipo = _mapper.Map<TipoHabitacion>(request.TipoHabitacion);
            await _unitOfWork.TiposHabitacion.AddAsync(tipo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<int>.Success(tipo.TipoHabitacionId);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error al crear el tipo de habitacion: {ex.Message}");
        }
    }
}
