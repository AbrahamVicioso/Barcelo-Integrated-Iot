using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Puesto;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Puesto.Queries.GetAllPuestos;

public class GetAllPuestosQueryHandler : IRequestHandler<GetAllPuestosQuery, IEnumerable<PuestoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllPuestosQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PuestoDto>> Handle(GetAllPuestosQuery request, CancellationToken cancellationToken)
    {
        var puestos = await _unitOfWork.Puestos.GetActivosAsync();
        return _mapper.Map<IEnumerable<PuestoDto>>(puestos);
    }
}
