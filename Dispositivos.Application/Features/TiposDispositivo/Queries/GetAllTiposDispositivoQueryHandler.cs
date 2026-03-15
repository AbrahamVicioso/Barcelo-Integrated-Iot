using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.TiposDispositivo.Queries;

public class GetAllTiposDispositivoQueryHandler : IRequestHandler<GetAllTiposDispositivoQuery, Result<IEnumerable<TipoDispositivoDto>>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public GetAllTiposDispositivoQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<TipoDispositivoDto>>> Handle(GetAllTiposDispositivoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tipos = await _unitOfWork.TiposDispositivo.GetAll();
            return Result<IEnumerable<TipoDispositivoDto>>.Success(_mapper.Map<IEnumerable<TipoDispositivoDto>>(tipos));
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TipoDispositivoDto>>.Failure($"Error al obtener los tipos: {ex.Message}");
        }
    }
}
