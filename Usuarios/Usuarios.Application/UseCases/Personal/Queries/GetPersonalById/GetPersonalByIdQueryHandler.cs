using AutoMapper;
using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.Personal;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Personal.Queries.GetPersonalById;

public class GetPersonalByIdQueryHandler : IRequestHandler<GetPersonalByIdQuery, Result<PersonalDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPersonalByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PersonalDto>> Handle(GetPersonalByIdQuery request, CancellationToken cancellationToken)
    {
        var personal = await _unitOfWork.Personal.GetByIdAsync(request.PersonalId);
        if (personal == null)
        {
            return Result<PersonalDto>.Failure("Personal no encontrado");
        }

        var personalDto = _mapper.Map<PersonalDto>(personal);
        return Result<PersonalDto>.Success(personalDto);
    }
}
