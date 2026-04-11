using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.PermisosPersonal;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.PermisosPersonal.Queries.GetPermisosByHabitacion;

public class GetPermisosByHabitacionQueryHandler : IRequestHandler<GetPermisosByHabitacionQuery, IEnumerable<PermisosPersonalDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPermisosByHabitacionQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PermisosPersonalDto>> Handle(GetPermisosByHabitacionQuery request, CancellationToken cancellationToken)
    {
        var permisos = await _unitOfWork.PermisosPersonal.GetPermisosByHabitacionAsync(request.HabitacionId);
        return _mapper.Map<IEnumerable<PermisosPersonalDto>>(permisos);
    }
}
