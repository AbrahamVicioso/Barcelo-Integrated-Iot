using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.TipoDocumento;
using Usuarios.Application.Exceptions;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.TipoDocumento.Commands.CreateTipoDocumento;

public class CreateTipoDocumentoCommandHandler : IRequestHandler<CreateTipoDocumentoCommand, TipoDocumentoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateTipoDocumentoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<TipoDocumentoDto> Handle(CreateTipoDocumentoCommand request, CancellationToken cancellationToken)
    {
        var existente = await _unitOfWork.TiposDocumento.GetByNombreAsync(request.TipoDocumento.Nombre);
        if (existente != null)
            throw new ConflictException($"Ya existe un tipo de documento con el nombre '{request.TipoDocumento.Nombre}'");

        var tipo = _mapper.Map<Domain.Entities.TipoDocumento>(request.TipoDocumento);
        tipo.EstaActivo = true;
        tipo.FechaCreacion = DateTime.Now;

        await _unitOfWork.TiposDocumento.AddAsync(tipo);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<TipoDocumentoDto>(tipo);
    }
}
