using AutoMapper;
using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.PermisosPersonal;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.PermisosPersonal.Queries.GetPermisosByPersonal;

public class GetPermisosByPersonalQueryHandler : IRequestHandler<GetPermisosByPersonalQuery, Result<IEnumerable<PermisosPersonalDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPermisosByPersonalQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<PermisosPersonalDto>>> Handle(GetPermisosByPersonalQuery request, CancellationToken cancellationToken)
    {
        var permisos = await _unitOfWork.PermisosPersonal.GetByPersonalIdAsync(request.PersonalId);
        var permisosDto = _mapper.Map<IEnumerable<PermisosPersonalDto>>(permisos);
        return Result<IEnumerable<PermisosPersonalDto>>.Success(permisosDto);
    }
}
