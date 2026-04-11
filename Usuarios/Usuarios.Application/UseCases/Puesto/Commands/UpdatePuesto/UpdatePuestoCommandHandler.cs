using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Puesto;
using Usuarios.Application.Exceptions;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Puesto.Commands.UpdatePuesto;

public class UpdatePuestoCommandHandler : IRequestHandler<UpdatePuestoCommand, PuestoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdatePuestoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PuestoDto> Handle(UpdatePuestoCommand request, CancellationToken cancellationToken)
    {
        var puesto = await _unitOfWork.Puestos.GetByIdAsync(request.Puesto.PuestoId);
        if (puesto == null)
            throw new NotFoundException($"Puesto con ID {request.Puesto.PuestoId} no encontrado");

        var existente = await _unitOfWork.Puestos.GetByNombreAsync(request.Puesto.Nombre);
        if (existente != null && existente.PuestoId != request.Puesto.PuestoId)
            throw new ConflictException($"Ya existe un puesto con el nombre '{request.Puesto.Nombre}'");

        puesto.Nombre = request.Puesto.Nombre;
        puesto.Descripcion = request.Puesto.Descripcion;
        puesto.EstaActivo = request.Puesto.EstaActivo;

        await _unitOfWork.Puestos.UpdateAsync(puesto);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PuestoDto>(puesto);
    }
}
