using AutoMapper;
using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.Huespedes;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Huespedes.Queries.GetAllHuespedes;

public class GetAllHuespedesQueryHandler : IRequestHandler<GetAllHuespedesQuery, Result<IEnumerable<HuespedeDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllHuespedesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<HuespedeDto>>> Handle(GetAllHuespedesQuery request, CancellationToken cancellationToken)
    {
        var huespedes = await _unitOfWork.Huespedes.GetAllAsync();
        var huespedesDto = _mapper.Map<IEnumerable<HuespedeDto>>(huespedes);
        return Result<IEnumerable<HuespedeDto>>.Success(huespedesDto);
    }
}
