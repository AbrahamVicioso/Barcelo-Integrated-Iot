using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Huespedes;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Huespedes.Queries.GetHuespedeByUserId;

public class GetHuespedeByUserIdQueryHandler : IRequestHandler<GetHuespedeByUserIdQuery, HuespedeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetHuespedeByUserIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<HuespedeDto> Handle(GetHuespedeByUserIdQuery request, CancellationToken cancellationToken)
    {
        var huespede = await _unitOfWork.Huespedes.GetByUsuarioIdAsync(request.UsuarioId);
        if (huespede == null)
        {
            throw new Exception("Huésped no encontrado para el usuario especificado");
        }

        var huespedeDto = _mapper.Map<HuespedeDto>(huespede);
        return huespedeDto;
    }
}
