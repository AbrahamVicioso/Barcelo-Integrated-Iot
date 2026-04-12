using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.TipoDocumento;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.TipoDocumento.Queries.GetAllTiposDocumento;

public class GetAllTiposDocumentoQueryHandler : IRequestHandler<GetAllTiposDocumentoQuery, IEnumerable<TipoDocumentoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllTiposDocumentoQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TipoDocumentoDto>> Handle(GetAllTiposDocumentoQuery request, CancellationToken cancellationToken)
    {
        var tipos = await _unitOfWork.TiposDocumento.GetActivosAsync();
        return _mapper.Map<IEnumerable<TipoDocumentoDto>>(tipos);
    }
}
