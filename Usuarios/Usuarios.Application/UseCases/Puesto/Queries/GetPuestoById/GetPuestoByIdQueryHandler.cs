using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Puesto;
using Usuarios.Application.Exceptions;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Puesto.Queries.GetPuestoById;

public class GetPuestoByIdQueryHandler : IRequestHandler<GetPuestoByIdQuery, PuestoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPuestoByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PuestoDto> Handle(GetPuestoByIdQuery request, CancellationToken cancellationToken)
    {
        var puesto = await _unitOfWork.Puestos.GetByIdAsync(request.PuestoId);
        if (puesto == null)
            throw new NotFoundException($"Puesto con ID {request.PuestoId} no encontrado");

        return _mapper.Map<PuestoDto>(puesto);
    }
}
