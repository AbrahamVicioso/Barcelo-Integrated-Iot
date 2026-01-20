using AutoMapper;
using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.Personal;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Personal.Queries.GetPersonalByDepartamento;

public class GetPersonalByDepartamentoQueryHandler : IRequestHandler<GetPersonalByDepartamentoQuery, Result<IEnumerable<PersonalDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPersonalByDepartamentoQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<PersonalDto>>> Handle(GetPersonalByDepartamentoQuery request, CancellationToken cancellationToken)
    {
        var personal = await _unitOfWork.Personal.GetByDepartamentoAsync(request.Departamento);
        var personalDto = _mapper.Map<IEnumerable<PersonalDto>>(personal);
        return Result<IEnumerable<PersonalDto>>.Success(personalDto);
    }
}
