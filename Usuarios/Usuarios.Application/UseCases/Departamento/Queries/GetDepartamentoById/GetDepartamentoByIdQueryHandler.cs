using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Departamento;
using Usuarios.Application.Exceptions;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Departamento.Queries.GetDepartamentoById;

public class GetDepartamentoByIdQueryHandler : IRequestHandler<GetDepartamentoByIdQuery, DepartamentoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetDepartamentoByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<DepartamentoDto> Handle(GetDepartamentoByIdQuery request, CancellationToken cancellationToken)
    {
        var departamento = await _unitOfWork.Departamentos.GetByIdAsync(request.DepartamentoId);
        if (departamento == null)
            throw new NotFoundException($"Departamento con ID {request.DepartamentoId} no encontrado");

        return _mapper.Map<DepartamentoDto>(departamento);
    }
}
