using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Personal;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Personal.Queries.GetPersonalById;

public class GetPersonalByIdQueryHandler : IRequestHandler<GetPersonalByIdQuery, PersonalDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPersonalByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PersonalDto> Handle(GetPersonalByIdQuery request, CancellationToken cancellationToken)
    {
        var personal = await _unitOfWork.Personal.GetByIdAsync(request.PersonalId);
        if (personal == null)
        {
            throw new Exception("Personal no encontrado");
        }

        var personalDto = _mapper.Map<PersonalDto>(personal);
        return personalDto;
    }
}
