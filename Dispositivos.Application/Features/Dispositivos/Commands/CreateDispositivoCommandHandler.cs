using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;
using Dispositivos.Domain.Entities;

namespace Dispositivos.Application.Features.Dispositivos.Commands;

public class CreateDispositivoCommandHandler : IRequestHandler<CreateDispositivoCommand, Result<Guid>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDispositivoCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateDispositivoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var dispositivo = _mapper.Map<Dispositivo>(request.Dispositivo);
            dispositivo.FechaCreacion = DateTime.UtcNow;

            await _unitOfWork.Dispositivos.AddAsync(dispositivo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(dispositivo.DispositivoId);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Error al crear el dispositivo: {ex.Message}");
        }
    }
}
