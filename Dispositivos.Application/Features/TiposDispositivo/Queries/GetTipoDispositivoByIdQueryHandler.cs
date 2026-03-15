using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.TiposDispositivo.Queries;

public class GetTipoDispositivoByIdQueryHandler : IRequestHandler<GetTipoDispositivoByIdQuery, Result<TipoDispositivoDto>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public GetTipoDispositivoByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TipoDispositivoDto>> Handle(GetTipoDispositivoByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tipo = await _unitOfWork.TiposDispositivo.GetById(request.TipoDispositivoId);
            if (tipo == null)
                return Result<TipoDispositivoDto>.Failure($"Tipo con ID {request.TipoDispositivoId} no encontrado.");

            return Result<TipoDispositivoDto>.Success(_mapper.Map<TipoDispositivoDto>(tipo));
        }
        catch (Exception ex)
        {
            return Result<TipoDispositivoDto>.Failure($"Error al obtener el tipo: {ex.Message}");
        }
    }
}
