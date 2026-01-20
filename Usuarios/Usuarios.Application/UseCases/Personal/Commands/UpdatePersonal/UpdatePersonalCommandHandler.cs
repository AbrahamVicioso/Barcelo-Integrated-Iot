using AutoMapper;
using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.Personal;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Personal.Commands.UpdatePersonal;

public class UpdatePersonalCommandHandler : IRequestHandler<UpdatePersonalCommand, Result<PersonalDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdatePersonalCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PersonalDto>> Handle(UpdatePersonalCommand request, CancellationToken cancellationToken)
    {
        var personal = await _unitOfWork.Personal.GetByIdAsync(request.Personal.PersonalId);
        if (personal == null)
        {
            return Result<PersonalDto>.Failure("Personal no encontrado");
        }

        if (request.Personal.Supervisor.HasValue)
        {
            var supervisor = await _unitOfWork.Personal.GetByIdAsync(request.Personal.Supervisor.Value);
            if (supervisor == null)
            {
                return Result<PersonalDto>.Failure("El supervisor especificado no existe");
            }

            if (request.Personal.Supervisor.Value == request.Personal.PersonalId)
            {
                return Result<PersonalDto>.Failure("El personal no puede ser su propio supervisor");
            }
        }

        personal.NombreCompleto = request.Personal.NombreCompleto;
        personal.Puesto = request.Personal.Puesto;
        personal.Departamento = request.Personal.Departamento;
        personal.EstaActivo = request.Personal.EstaActivo;
        personal.Turno = request.Personal.Turno;
        personal.Supervisor = request.Personal.Supervisor;

        await _unitOfWork.Personal.UpdateAsync(personal);
        await _unitOfWork.SaveChangesAsync();

        var personalDto = _mapper.Map<PersonalDto>(personal);
        return Result<PersonalDto>.Success(personalDto, "Personal actualizado exitosamente");
    }
}
