using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Puesto;
using Usuarios.Application.Exceptions;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Puesto.Commands.CreatePuesto;

public class CreatePuestoCommandHandler : IRequestHandler<CreatePuestoCommand, PuestoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreatePuestoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PuestoDto> Handle(CreatePuestoCommand request, CancellationToken cancellationToken)
    {
        var existente = await _unitOfWork.Puestos.GetByNombreAsync(request.Puesto.Nombre);
        if (existente != null)
            throw new ConflictException($"Ya existe un puesto con el nombre '{request.Puesto.Nombre}'");

        var puesto = _mapper.Map<Domain.Entities.Puesto>(request.Puesto);
        puesto.EstaActivo = true;
        puesto.FechaCreacion = DateTime.Now;

        await _unitOfWork.Puestos.AddAsync(puesto);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PuestoDto>(puesto);
    }
}
