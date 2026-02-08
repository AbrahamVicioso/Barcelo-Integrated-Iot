using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.Dispositivos.Queries;

public class GetAllDispositivosQueryHandler : IRequestHandler<GetAllDispositivosQuery, Result<IEnumerable<DispositivoDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllDispositivosQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<DispositivoDto>>> Handle(GetAllDispositivosQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var dispositivos = await _unitOfWork.Dispositivos.GetAll();
            var dispositivosDto = _mapper.Map<IEnumerable<DispositivoDto>>(dispositivos);
            return Result<IEnumerable<DispositivoDto>>.Success(dispositivosDto);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<DispositivoDto>>.Failure($"Error al obtener los dispositivos: {ex.Message}");
        }
    }
}
