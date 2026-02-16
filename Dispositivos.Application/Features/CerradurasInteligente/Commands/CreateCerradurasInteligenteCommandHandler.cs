using AutoMapper;
using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;
using CerraduraEntity = Dispositivos.Domain.Entities.CerradurasInteligente;

namespace Dispositivos.Application.Features.CerradurasInteligente.Commands;

public class CreateCerradurasInteligenteCommandHandler : IRequestHandler<CreateCerradurasInteligenteCommand, Result<int>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICerradurasInteligenteRepository _cerraduraRepository;

    public CreateCerradurasInteligenteCommandHandler(
        IMapper mapper, 
        IUnitOfWork unitOfWork,
        ICerradurasInteligenteRepository cerraduraRepository)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _cerraduraRepository = cerraduraRepository;
    }

    public async Task<Result<int>> Handle(CreateCerradurasInteligenteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var cerradura = _mapper.Map<CerraduraEntity>(request.Cerradura);
            
            await _cerraduraRepository.AddAsync(cerradura, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(cerradura.CerraduraId);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error al crear la cerradura inteligente: {ex.Message}");
        }
    }
}
