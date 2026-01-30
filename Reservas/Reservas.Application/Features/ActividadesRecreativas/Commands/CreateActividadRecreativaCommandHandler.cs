using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entites;

namespace Reservas.Application.Features.ActividadesRecreativas.Commands;

public class CreateActividadRecreativaCommandHandler : IRequestHandler<CreateActividadRecreativaCommand, Result<ActividadRecreativaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateActividadRecreativaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ActividadRecreativaDto>> Handle(CreateActividadRecreativaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var actividad = _mapper.Map<Domain.Entites.ActividadesRecreativas>(request);
            actividad.EstaActiva = true;
            actividad.FechaCreacion = DateTime.UtcNow;

            await _unitOfWork.ActividadesRecreativas.AddAsync(actividad, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var actividadDto = _mapper.Map<ActividadRecreativaDto>(actividad);
            return Result<ActividadRecreativaDto>.Success(actividadDto);
        }
        catch (Exception ex)
        {
            return Result<ActividadRecreativaDto>.Failure($"Error al crear la actividad: {ex.Message}");
        }
    }
}
