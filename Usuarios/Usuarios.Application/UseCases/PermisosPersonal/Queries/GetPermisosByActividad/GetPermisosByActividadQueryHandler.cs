using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.PermisosPersonal;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.PermisosPersonal.Queries.GetPermisosByActividad;

public class GetPermisosByActividadQueryHandler : IRequestHandler<GetPermisosByActividadQuery, IEnumerable<PermisosPersonalDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPermisosByActividadQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PermisosPersonalDto>> Handle(GetPermisosByActividadQuery request, CancellationToken cancellationToken)
    {
        var permisos = await _unitOfWork.PermisosPersonal.GetPermisosByActividadAsync(request.ActividadId);
        return _mapper.Map<IEnumerable<PermisosPersonalDto>>(permisos);
    }
}
