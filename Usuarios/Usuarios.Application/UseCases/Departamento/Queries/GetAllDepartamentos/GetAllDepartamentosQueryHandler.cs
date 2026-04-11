using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.Departamento;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Departamento.Queries.GetAllDepartamentos;

public class GetAllDepartamentosQueryHandler : IRequestHandler<GetAllDepartamentosQuery, IEnumerable<DepartamentoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllDepartamentosQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DepartamentoDto>> Handle(GetAllDepartamentosQuery request, CancellationToken cancellationToken)
    {
        var departamentos = await _unitOfWork.Departamentos.GetActivosAsync();
        return _mapper.Map<IEnumerable<DepartamentoDto>>(departamentos);
    }
}
