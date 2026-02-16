using MediatR;
using AutoMapper;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.RegistrosAuditorium.Queries;

public class GetRegistrosAuditoriumByIdQueryHandler : IRequestHandler<GetRegistrosAuditoriumByIdQuery, Result<RegistrosAuditoriumDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetRegistrosAuditoriumByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<RegistrosAuditoriumDto>> Handle(GetRegistrosAuditoriumByIdQuery request, CancellationToken cancellationToken)
    {
        var registro = await _unitOfWork.RegistrosAuditorium.GetByIdAsync(request.RegistroId);
        
        if (registro == null)
        {
            return Result<RegistrosAuditoriumDto>.Failure("Registro de auditoría no encontrado.");
        }

        var dto = _mapper.Map<RegistrosAuditoriumDto>(registro);
        return Result<RegistrosAuditoriumDto>.Success(dto);
    }
}
