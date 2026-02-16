using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.CerradurasInteligente.Commands;

public class DeleteCerradurasInteligenteCommandHandler : IRequestHandler<DeleteCerradurasInteligenteCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICerradurasInteligenteRepository _cerraduraRepository;

    public DeleteCerradurasInteligenteCommandHandler(
        IUnitOfWork unitOfWork,
        ICerradurasInteligenteRepository cerraduraRepository)
    {
        _unitOfWork = unitOfWork;
        _cerraduraRepository = cerraduraRepository;
    }

    public async Task<Result<bool>> Handle(DeleteCerradurasInteligenteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var cerradura = await _cerraduraRepository.GetById(request.CerraduraId);

            if (cerradura == null)
            {
                return Result<bool>.Failure($"Cerradura con ID {request.CerraduraId} no encontrada.");
            }

            await _cerraduraRepository.DeleteAsync(cerradura, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error al eliminar la cerradura inteligente: {ex.Message}");
        }
    }
}
