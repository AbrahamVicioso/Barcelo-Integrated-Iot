using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Personal;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Personal.Queries.GetPersonalByDepartamento;

public class GetPersonalByDepartamentoQueryHandler : IRequestHandler<GetPersonalByDepartamentoQuery, IEnumerable<PersonalDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPersonalByDepartamentoQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PersonalDto>> Handle(GetPersonalByDepartamentoQuery request, CancellationToken cancellationToken)
    {
        var personal = await _unitOfWork.Personal.GetByDepartamentoIdAsync(request.DepartamentoId);
        return _mapper.Map<IEnumerable<PersonalDto>>(personal);
    }
}
