using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.Hoteles.Queries;

public class GetAllHotelesQueryHandler : IRequestHandler<GetAllHotelesQuery, Result<IEnumerable<HotelesDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllHotelesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<HotelesDto>>> Handle(GetAllHotelesQuery request, CancellationToken cancellationToken)
    {
        var hoteles = await _unitOfWork.Hoteles.GetAll();
        var mappeds = hoteles.Select(hotel => _mapper.Map<HotelesDto>(hotel));
        return Result<IEnumerable<HotelesDto>>.Success(mappeds);
    }
}
