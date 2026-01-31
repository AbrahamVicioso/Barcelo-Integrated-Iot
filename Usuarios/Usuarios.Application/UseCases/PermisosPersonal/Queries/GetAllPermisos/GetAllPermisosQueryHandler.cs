using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.PermisosPersonal;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.PermisosPersonal.Queries.GetAllPermisos;

public class GetAllPermisosQueryHandler : IRequestHandler<GetAllPermisosQuery, IEnumerable<PermisosPersonalDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllPermisosQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PermisosPersonalDto>> Handle(GetAllPermisosQuery request, CancellationToken cancellationToken)
    {
        var permisos = await _unitOfWork.PermisosPersonal.GetAllAsync();
        var permisosDto = _mapper.Map<IEnumerable<PermisosPersonalDto>>(permisos);
        return permisosDto;
    }
}
