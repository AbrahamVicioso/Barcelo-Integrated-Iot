using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.TipoDocumento;
using Usuarios.Application.Exceptions;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.TipoDocumento.Queries.GetTipoDocumentoById;

public class GetTipoDocumentoByIdQueryHandler : IRequestHandler<GetTipoDocumentoByIdQuery, TipoDocumentoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetTipoDocumentoByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<TipoDocumentoDto> Handle(GetTipoDocumentoByIdQuery request, CancellationToken cancellationToken)
    {
        var tipo = await _unitOfWork.TiposDocumento.GetByIdAsync(request.TipoDocumentoId);
        if (tipo == null)
            throw new NotFoundException($"Tipo de documento con ID {request.TipoDocumentoId} no encontrado");

        return _mapper.Map<TipoDocumentoDto>(tipo);
    }
}
