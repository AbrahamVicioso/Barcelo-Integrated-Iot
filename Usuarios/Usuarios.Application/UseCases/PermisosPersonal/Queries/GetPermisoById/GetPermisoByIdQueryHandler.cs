using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.PermisosPersonal;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.PermisosPersonal.Queries.GetPermisoById;

public class GetPermisoByIdQueryHandler : IRequestHandler<GetPermisoByIdQuery, PermisosPersonalDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPermisoByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PermisosPersonalDto> Handle(GetPermisoByIdQuery request, CancellationToken cancellationToken)
    {
        var permiso = await _unitOfWork.PermisosPersonal.GetByIdAsync(request.PermisoId);
        if (permiso == null)
        {
            throw new Exception("Permiso no encontrado");
        }

        var permisoDto = _mapper.Map<PermisosPersonalDto>(permiso);
        return permisoDto;
    }
}
