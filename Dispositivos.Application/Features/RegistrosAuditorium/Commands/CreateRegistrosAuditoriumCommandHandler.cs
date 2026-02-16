using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;
using RegistroEntity = Dispositivos.Domain.Entities.RegistrosAuditorium;

namespace Dispositivos.Application.Features.RegistrosAuditorium.Commands;

public class CreateRegistrosAuditoriumCommandHandler : IRequestHandler<CreateRegistrosAuditoriumCommand, Result<int>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRegistrosAuditoriumRepository _registroRepository;

    public CreateRegistrosAuditoriumCommandHandler(
        IMapper mapper, 
        IUnitOfWork unitOfWork,
        IRegistrosAuditoriumRepository registroRepository)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _registroRepository = registroRepository;
    }

    public async Task<Result<int>> Handle(CreateRegistrosAuditoriumCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var registro = _mapper.Map<RegistroEntity>(request.Registro);
            
            await _registroRepository.AddAsync(registro, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<int>.Success((int)registro.AuditoriaId);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error al crear el registro de auditoría: {ex.Message}");
        }
    }
}
