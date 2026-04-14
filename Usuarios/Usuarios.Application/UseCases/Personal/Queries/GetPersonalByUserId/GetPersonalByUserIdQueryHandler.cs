using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Personal;
using Usuarios.Application.Exceptions;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Personal.Queries.GetPersonalByUserId;

public class GetPersonalByUserIdQueryHandler : IRequestHandler<GetPersonalByUserIdQuery, PersonalDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPersonalByUserIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PersonalDto> Handle(GetPersonalByUserIdQuery request, CancellationToken cancellationToken)
    {
        var personal = await _unitOfWork.Personal.GetByUsuarioIdAsync(request.UsuarioId);
        if (personal == null)
            throw new NotFoundException("Personal no encontrado para el usuario especificado");

        return _mapper.Map<PersonalDto>(personal);
    }
}
