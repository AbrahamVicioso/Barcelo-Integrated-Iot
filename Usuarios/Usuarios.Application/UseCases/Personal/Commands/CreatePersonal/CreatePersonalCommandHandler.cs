using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Personal;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Personal.Commands.CreatePersonal;

public class CreatePersonalCommandHandler : IRequestHandler<CreatePersonalCommand, PersonalDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreatePersonalCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PersonalDto> Handle(CreatePersonalCommand request, CancellationToken cancellationToken)
    {
        var existingByUsuario = await _unitOfWork.Personal.GetByUsuarioIdAsync(request.Personal.UsuarioId);
        if (existingByUsuario != null)
        {
            throw new Exception("Ya existe personal con ese UsuarioId");
        }

        var existingByNumeroEmpleado = await _unitOfWork.Personal.GetByNumeroEmpleadoAsync(request.Personal.NumeroEmpleado);
        if (existingByNumeroEmpleado != null)
        {
            throw new Exception("Ya existe personal con ese número de empleado");
        }

        if (request.Personal.Supervisor.HasValue)
        {
            var supervisor = await _unitOfWork.Personal.GetByIdAsync(request.Personal.Supervisor.Value);
            if (supervisor == null)
            {
                throw new Exception("El supervisor especificado no existe");
            }
        }

        var personal = _mapper.Map<Domain.Entities.Personal>(request.Personal);
        personal.FechaCreacion = DateTime.UtcNow;
        personal.EstaActivo = true;

        var createdPersonal = await _unitOfWork.Personal.AddAsync(personal);
        await _unitOfWork.SaveChangesAsync();

        var personalDto = _mapper.Map<PersonalDto>(createdPersonal);
        return personalDto;
    }
}
