using AutoMapper;
using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.Personal;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Personal.Queries.GetAllPersonal;

public class GetAllPersonalQueryHandler : IRequestHandler<GetAllPersonalQuery, Result<IEnumerable<PersonalDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllPersonalQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<PersonalDto>>> Handle(GetAllPersonalQuery request, CancellationToken cancellationToken)
    {
        var personal = await _unitOfWork.Personal.GetAllAsync();
        var personalDto = _mapper.Map<IEnumerable<PersonalDto>>(personal);
        return Result<IEnumerable<PersonalDto>>.Success(personalDto);
    }
}
