using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;
using CredencialEntity = Dispositivos.Domain.Entities.CredencialesAcceso;

namespace Dispositivos.Application.Features.CredencialesAcceso.Commands;

public class CreateCredencialesAccesoCommandHandler : IRequestHandler<CreateCredencialesAccesoCommand, Result<int>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICredencialesAccesoRepository _credencialRepository;

    public CreateCredencialesAccesoCommandHandler(
        IMapper mapper, 
        IUnitOfWork unitOfWork,
        ICredencialesAccesoRepository credencialRepository)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _credencialRepository = credencialRepository;
    }

    public async Task<Result<int>> Handle(CreateCredencialesAccesoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var credencial = _mapper.Map<CredencialEntity>(request.Credencial);
            credencial.FechaCreacion = DateTime.UtcNow;
            
            await _credencialRepository.AddAsync(credencial, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(credencial.CredencialId);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error al crear la credencial de acceso: {ex.Message}");
        }
    }
}
