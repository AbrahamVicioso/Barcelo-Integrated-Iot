using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.Interfaces;
using Dispositivos.Domain.Entities;

namespace Dispositivos.Application.Features.TiposDispositivo.Commands;

public class CreateTipoDispositivoCommandHandler : IRequestHandler<CreateTipoDispositivoCommand, Result<int>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTipoDispositivoCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(CreateTipoDispositivoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tipo = _mapper.Map<TipoDispositivo>(request.TipoDispositivo);
            await _unitOfWork.TiposDispositivo.AddAsync(tipo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<int>.Success(tipo.TipoDispositivoId);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error al crear el tipo: {ex.Message}");
        }
    }
}
