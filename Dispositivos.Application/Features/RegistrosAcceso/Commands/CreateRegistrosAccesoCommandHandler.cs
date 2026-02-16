using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;
using RegistroEntity = Dispositivos.Domain.Entities.RegistrosAcceso;

namespace Dispositivos.Application.Features.RegistrosAcceso.Commands;

public class CreateRegistrosAccesoCommandHandler : IRequestHandler<CreateRegistrosAccesoCommand, Result<long>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRegistrosAccesoRepository _registroRepository;

    public CreateRegistrosAccesoCommandHandler(
        IMapper mapper, 
        IUnitOfWork unitOfWork,
        IRegistrosAccesoRepository registroRepository)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _registroRepository = registroRepository;
    }

    public async Task<Result<long>> Handle(CreateRegistrosAccesoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var registro = _mapper.Map<RegistroEntity>(request.Registro);
            
            await _registroRepository.AddAsync(registro, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<long>.Success(registro.RegistroId);
        }
        catch (Exception ex)
        {
            return Result<long>.Failure($"Error al crear el registro de acceso: {ex.Message}");
        }
    }
}
