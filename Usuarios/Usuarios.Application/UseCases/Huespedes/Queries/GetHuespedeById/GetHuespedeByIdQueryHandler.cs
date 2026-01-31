using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Huespedes;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Huespedes.Queries.GetHuespedeById;

public class GetHuespedeByIdQueryHandler : IRequestHandler<GetHuespedeByIdQuery, HuespedeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetHuespedeByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<HuespedeDto> Handle(GetHuespedeByIdQuery request, CancellationToken cancellationToken)
    {
        var huespede = await _unitOfWork.Huespedes.GetByIdAsync(request.HuespedId);
        if (huespede == null)
        {
            throw new Exception("Huésped no encontrado");
        }

        var huespedeDto = _mapper.Map<HuespedeDto>(huespede);
        return huespedeDto;
    }
}
