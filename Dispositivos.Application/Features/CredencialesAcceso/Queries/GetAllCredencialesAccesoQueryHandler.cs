using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.CredencialesAcceso.Queries;

public class GetAllCredencialesAccesoQueryHandler : IRequestHandler<GetAllCredencialesAccesoQuery, Result<IEnumerable<CredencialesAccesoDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICredencialesAccesoRepository _credencialRepository;

    public GetAllCredencialesAccesoQueryHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        ICredencialesAccesoRepository credencialRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _credencialRepository = credencialRepository;
    }

    public async Task<Result<IEnumerable<CredencialesAccesoDto>>> Handle(GetAllCredencialesAccesoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var credenciales = await _credencialRepository.GetAll();
            var credencialesDto = _mapper.Map<IEnumerable<CredencialesAccesoDto>>(credenciales);
            return Result<IEnumerable<CredencialesAccesoDto>>.Success(credencialesDto);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<CredencialesAccesoDto>>.Failure($"Error al obtener las credenciales de acceso: {ex.Message}");
        }
    }
}
