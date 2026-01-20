using AutoMapper;
using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.Huespedes;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Huespedes.Queries.GetHuespedeById;

public class GetHuespedeByIdQueryHandler : IRequestHandler<GetHuespedeByIdQuery, Result<HuespedeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetHuespedeByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<HuespedeDto>> Handle(GetHuespedeByIdQuery request, CancellationToken cancellationToken)
    {
        var huespede = await _unitOfWork.Huespedes.GetByIdAsync(request.HuespedId);
        if (huespede == null)
        {
            return Result<HuespedeDto>.Failure("Huésped no encontrado");
        }

        var huespedeDto = _mapper.Map<HuespedeDto>(huespede);
        return Result<HuespedeDto>.Success(huespedeDto);
    }
}
