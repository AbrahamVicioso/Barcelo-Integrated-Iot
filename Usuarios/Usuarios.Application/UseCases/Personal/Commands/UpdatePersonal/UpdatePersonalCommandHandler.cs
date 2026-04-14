using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Personal;
using Usuarios.Application.Exceptions;
using Usuarios.Application.Interfaces;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Personal.Commands.UpdatePersonal;

public class UpdatePersonalCommandHandler : IRequestHandler<UpdatePersonalCommand, PersonalDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDispositivosApiService _dispositivosApi;

    public UpdatePersonalCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDispositivosApiService dispositivosApi)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dispositivosApi = dispositivosApi;
    }

    public async Task<PersonalDto> Handle(UpdatePersonalCommand request, CancellationToken cancellationToken)
    {
        var personal = await _unitOfWork.Personal.GetByIdAsync(request.Personal.PersonalId);
        if (personal == null)
        {
            throw new NotFoundException("Personal no encontrado");
        }

        var puesto = await _unitOfWork.Puestos.GetByIdAsync(request.Personal.PuestoId);
        if (puesto == null || !puesto.EstaActivo)
            throw new NotFoundException($"Puesto con ID {request.Personal.PuestoId} no encontrado o inactivo");

        var departamento = await _unitOfWork.Departamentos.GetByIdAsync(request.Personal.DepartamentoId);
        if (departamento == null || !departamento.EstaActivo)
            throw new NotFoundException($"Departamento con ID {request.Personal.DepartamentoId} no encontrado o inactivo");

        if (request.Personal.Supervisor.HasValue)
        {
            var supervisor = await _unitOfWork.Personal.GetByIdAsync(request.Personal.Supervisor.Value);
            if (supervisor == null)
            {
                throw new NotFoundException("El supervisor especificado no existe");
            }

            if (request.Personal.Supervisor.Value == request.Personal.PersonalId)
            {
                throw new BusinessException("El personal no puede ser su propio supervisor");
            }
        }

        var estabaActivo = personal.EstaActivo;

        personal.NombreCompleto = request.Personal.NombreCompleto;
        personal.PuestoId = request.Personal.PuestoId;
        personal.DepartamentoId = request.Personal.DepartamentoId;
        personal.EstaActivo = request.Personal.EstaActivo;
        personal.Turno = request.Personal.Turno;
        personal.Supervisor = request.Personal.Supervisor;

        await _unitOfWork.Personal.UpdateAsync(personal);
        await _unitOfWork.SaveChangesAsync();

        if (estabaActivo != request.Personal.EstaActivo)
            await _dispositivosApi.SincronizarEstadoPersonalAsync(request.Personal.PersonalId, request.Personal.EstaActivo, cancellationToken);

        var personalDto = _mapper.Map<PersonalDto>(personal);
        return personalDto;
    }
}
