using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.CerradurasInteligente.Queries;

public class GetCerradurasInteligenteByIdQueryHandler : IRequestHandler<GetCerradurasInteligenteByIdQuery, Result<CerradurasInteligenteDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICerradurasInteligenteRepository _cerraduraRepository;

    public GetCerradurasInteligenteByIdQueryHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        ICerradurasInteligenteRepository cerraduraRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cerraduraRepository = cerraduraRepository;
    }

    public async Task<Result<CerradurasInteligenteDto>> Handle(GetCerradurasInteligenteByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var cerradura = await _cerraduraRepository.GetById(request.CerraduraId);

            if (cerradura == null)
            {
                return Result<CerradurasInteligenteDto>.Failure($"Cerradura con ID {request.CerraduraId} no encontrada.");
            }

            var cerraduraDto = _mapper.Map<CerradurasInteligenteDto>(cerradura);
            return Result<CerradurasInteligenteDto>.Success(cerraduraDto);
        }
        catch (Exception ex)
        {
            return Result<CerradurasInteligenteDto>.Failure($"Error al obtener la cerradura inteligente: {ex.Message}");
        }
    }
}
