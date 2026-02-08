using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.Dispositivos.Queries;

public class GetDispositivosByHotelIdQueryHandler : IRequestHandler<GetDispositivosByHotelIdQuery, Result<IEnumerable<DispositivoDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetDispositivosByHotelIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<DispositivoDto>>> Handle(GetDispositivosByHotelIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var dispositivos = await _unitOfWork.Dispositivos.GetByHotelId(request.HotelId);
            var dispositivosDto = _mapper.Map<IEnumerable<DispositivoDto>>(dispositivos);
            return Result<IEnumerable<DispositivoDto>>.Success(dispositivosDto);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<DispositivoDto>>.Failure($"Error al obtener los dispositivos por hotel: {ex.Message}");
        }
    }
}
