using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Departamento;
using Usuarios.Application.Exceptions;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Departamento.Commands.CreateDepartamento;

public class CreateDepartamentoCommandHandler : IRequestHandler<CreateDepartamentoCommand, DepartamentoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateDepartamentoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<DepartamentoDto> Handle(CreateDepartamentoCommand request, CancellationToken cancellationToken)
    {
        var existente = await _unitOfWork.Departamentos.GetByNombreAsync(request.Departamento.Nombre);
        if (existente != null)
            throw new ConflictException($"Ya existe un departamento con el nombre '{request.Departamento.Nombre}'");

        var departamento = _mapper.Map<Domain.Entities.Departamento>(request.Departamento);
        departamento.EstaActivo = true;
        departamento.FechaCreacion = DateTime.UtcNow;

        await _unitOfWork.Departamentos.AddAsync(departamento);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<DepartamentoDto>(departamento);
    }
}
