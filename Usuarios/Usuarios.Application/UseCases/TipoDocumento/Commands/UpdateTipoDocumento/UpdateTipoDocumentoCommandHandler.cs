using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.TipoDocumento;
using Usuarios.Application.Exceptions;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.TipoDocumento.Commands.UpdateTipoDocumento;

public class UpdateTipoDocumentoCommandHandler : IRequestHandler<UpdateTipoDocumentoCommand, TipoDocumentoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateTipoDocumentoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<TipoDocumentoDto> Handle(UpdateTipoDocumentoCommand request, CancellationToken cancellationToken)
    {
        var tipo = await _unitOfWork.TiposDocumento.GetByIdAsync(request.TipoDocumento.TipoDocumentoId);
        if (tipo == null)
            throw new NotFoundException($"Tipo de documento con ID {request.TipoDocumento.TipoDocumentoId} no encontrado");

        var existente = await _unitOfWork.TiposDocumento.GetByNombreAsync(request.TipoDocumento.Nombre);
        if (existente != null && existente.TipoDocumentoId != request.TipoDocumento.TipoDocumentoId)
            throw new ConflictException($"Ya existe un tipo de documento con el nombre '{request.TipoDocumento.Nombre}'");

        tipo.Nombre = request.TipoDocumento.Nombre;
        tipo.Descripcion = request.TipoDocumento.Descripcion;
        tipo.EstaActivo = request.TipoDocumento.EstaActivo;

        await _unitOfWork.TiposDocumento.UpdateAsync(tipo);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<TipoDocumentoDto>(tipo);
    }
}
