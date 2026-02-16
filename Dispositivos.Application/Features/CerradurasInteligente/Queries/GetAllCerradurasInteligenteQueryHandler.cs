using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.CerradurasInteligente.Queries;

public class GetAllCerradurasInteligenteQueryHandler : IRequestHandler<GetAllCerradurasInteligenteQuery, Result<IEnumerable<CerradurasInteligenteDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICerradurasInteligenteRepository _cerraduraRepository;

    public GetAllCerradurasInteligenteQueryHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        ICerradurasInteligenteRepository cerraduraRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cerraduraRepository = cerraduraRepository;
    }

    public async Task<Result<IEnumerable<CerradurasInteligenteDto>>> Handle(GetAllCerradurasInteligenteQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var cerraduras = await _cerraduraRepository.GetAll();
            var cerradurasDto = _mapper.Map<IEnumerable<CerradurasInteligenteDto>>(cerraduras);
            return Result<IEnumerable<CerradurasInteligenteDto>>.Success(cerradurasDto);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<CerradurasInteligenteDto>>.Failure($"Error al obtener las cerraduras inteligentes: {ex.Message}");
        }
    }
}
