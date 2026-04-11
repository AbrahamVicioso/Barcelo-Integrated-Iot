using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Departamento;
using Usuarios.Application.Exceptions;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Departamento.Commands.UpdateDepartamento;

public class UpdateDepartamentoCommandHandler : IRequestHandler<UpdateDepartamentoCommand, DepartamentoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateDepartamentoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<DepartamentoDto> Handle(UpdateDepartamentoCommand request, CancellationToken cancellationToken)
    {
        var departamento = await _unitOfWork.Departamentos.GetByIdAsync(request.Departamento.DepartamentoId);
        if (departamento == null)
            throw new NotFoundException($"Departamento con ID {request.Departamento.DepartamentoId} no encontrado");

        var existente = await _unitOfWork.Departamentos.GetByNombreAsync(request.Departamento.Nombre);
        if (existente != null && existente.DepartamentoId != request.Departamento.DepartamentoId)
            throw new ConflictException($"Ya existe un departamento con el nombre '{request.Departamento.Nombre}'");

        departamento.Nombre = request.Departamento.Nombre;
        departamento.Descripcion = request.Departamento.Descripcion;
        departamento.EstaActivo = request.Departamento.EstaActivo;

        await _unitOfWork.Departamentos.UpdateAsync(departamento);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<DepartamentoDto>(departamento);
    }
}
