using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entites;

namespace Reservas.Application.Features.EstadosReservaActividad.Commands;

public class CreateEstadoReservaActividadCommandHandler : IRequestHandler<CreateEstadoReservaActividadCommand, Result<EstadoReservaActividadDto>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEstadoReservaActividadCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<EstadoReservaActividadDto>> Handle(CreateEstadoReservaActividadCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = new EstadoReservaActividad
            {
                Nombre = request.Nombre,
                Descripcion = request.Descripcion
            };

            await _unitOfWork.EstadosReservaActividad.AddAsync(entity, cancellationToken);

            var dto = _mapper.Map<EstadoReservaActividadDto>(entity);
            return Result<EstadoReservaActividadDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<EstadoReservaActividadDto>.Failure($"Error al crear el estado: {ex.Message}");
        }
    }
}