using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.Interfaces;
using Dispositivos.Domain.Entities;

namespace Dispositivos.Application.Features.EstadosDispositivo.Commands;

public class CreateEstadoDispositivoCommandHandler : IRequestHandler<CreateEstadoDispositivoCommand, Result<int>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEstadoDispositivoCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(CreateEstadoDispositivoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var estado = _mapper.Map<EstadoDispositivo>(request.EstadoDispositivo);
            await _unitOfWork.EstadosDispositivo.AddAsync(estado, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<int>.Success(estado.EstadoDispositivoId);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error al crear el estado: {ex.Message}");
        }
    }
}
