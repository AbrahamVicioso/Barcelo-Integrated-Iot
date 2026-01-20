using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Domain.Interfaces;

namespace Reservas.Application.Features.ActividadesRecreativas.Queries;

public class GetAllActividadesQueryHandler : IRequestHandler<GetAllActividadesQuery, Result<IEnumerable<ActividadRecreativaDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllActividadesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<ActividadRecreativaDto>>> Handle(GetAllActividadesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var actividades = await _unitOfWork.ActividadesRecreativas.GetAllAsync(cancellationToken);
            var actividadesDto = _mapper.Map<IEnumerable<ActividadRecreativaDto>>(actividades);
            return Result<IEnumerable<ActividadRecreativaDto>>.Success(actividadesDto);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ActividadRecreativaDto>>.Failure($"Error al obtener las actividades: {ex.Message}");
        }
    }
}
