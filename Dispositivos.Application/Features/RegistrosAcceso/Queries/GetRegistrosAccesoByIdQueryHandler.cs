using MediatR;
using AutoMapper;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.RegistrosAcceso.Queries;

public class GetRegistrosAccesoByIdQueryHandler : IRequestHandler<GetRegistrosAccesoByIdQuery, Result<RegistrosAccesoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetRegistrosAccesoByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<RegistrosAccesoDto>> Handle(GetRegistrosAccesoByIdQuery request, CancellationToken cancellationToken)
    {
        var registro = await _unitOfWork.RegistrosAcceso.GetByIdAsync(request.RegistroId);
        
        if (registro == null)
        {
            return Result<RegistrosAccesoDto>.Failure("Registro de acceso no encontrado.");
        }

        var dto = _mapper.Map<RegistrosAccesoDto>(registro);
        return Result<RegistrosAccesoDto>.Success(dto);
    }
}
