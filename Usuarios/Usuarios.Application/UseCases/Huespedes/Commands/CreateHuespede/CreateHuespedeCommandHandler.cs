using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Huespedes;
using Usuarios.Domain.Entities;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Huespedes.Commands.CreateHuespede;

public class CreateHuespedeCommandHandler : IRequestHandler<CreateHuespedeCommand, HuespedeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateHuespedeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<HuespedeDto> Handle(CreateHuespedeCommand request, CancellationToken cancellationToken)
    {
        var existingByUsuario = await _unitOfWork.Huespedes.GetByUsuarioIdAsync(request.Huespede.UsuarioId);
        if (existingByUsuario != null)
        {
            throw new Exception("Ya existe un huésped con ese UsuarioId");
        }

        var existingByDocumento = await _unitOfWork.Huespedes.GetByDocumentoAsync(
            request.Huespede.TipoDocumento,
            request.Huespede.NumeroDocumento);
        if (existingByDocumento != null)
        {
            throw new Exception("Ya existe un huésped con ese documento");
        }

        var huespede = _mapper.Map<Huespede>(request.Huespede);
        huespede.FechaRegistro = DateTime.UtcNow;

        var createdHuespede = await _unitOfWork.Huespedes.AddAsync(huespede);
        await _unitOfWork.SaveChangesAsync();

        var huespedeDto = _mapper.Map<HuespedeDto>(createdHuespede);

        return huespedeDto;
    }
}
