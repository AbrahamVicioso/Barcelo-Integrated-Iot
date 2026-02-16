using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.CredencialesAcceso.Queries;

public class GetCredencialesAccesoByIdQueryHandler : IRequestHandler<GetCredencialesAccesoByIdQuery, Result<CredencialesAccesoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICredencialesAccesoRepository _credencialRepository;

    public GetCredencialesAccesoByIdQueryHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        ICredencialesAccesoRepository credencialRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _credencialRepository = credencialRepository;
    }

    public async Task<Result<CredencialesAccesoDto>> Handle(GetCredencialesAccesoByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var credencial = await _credencialRepository.GetById(request.CredencialId);

            if (credencial == null)
            {
                return Result<CredencialesAccesoDto>.Failure($"Credencial con ID {request.CredencialId} no encontrada.");
            }

            var credencialDto = _mapper.Map<CredencialesAccesoDto>(credencial);
            return Result<CredencialesAccesoDto>.Success(credencialDto);
        }
        catch (Exception ex)
        {
            return Result<CredencialesAccesoDto>.Failure($"Error al obtener la credencial de acceso: {ex.Message}");
        }
    }
}
