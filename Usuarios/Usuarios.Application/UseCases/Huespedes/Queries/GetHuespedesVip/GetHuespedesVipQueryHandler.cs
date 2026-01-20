using AutoMapper;
using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.Huespedes;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Huespedes.Queries.GetHuespedesVip;

public class GetHuespedesVipQueryHandler : IRequestHandler<GetHuespedesVipQuery, Result<IEnumerable<HuespedeDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetHuespedesVipQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<HuespedeDto>>> Handle(GetHuespedesVipQuery request, CancellationToken cancellationToken)
    {
        var huespedes = await _unitOfWork.Huespedes.GetHuespedesVipAsync();
        var huespedesDto = _mapper.Map<IEnumerable<HuespedeDto>>(huespedes);
        return Result<IEnumerable<HuespedeDto>>.Success(huespedesDto);
    }
}
