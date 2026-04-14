using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.CredencialesAcceso.Commands;

public class DeleteCredencialesAccesoCommandHandler : IRequestHandler<DeleteCredencialesAccesoCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICredencialesAccesoRepository _credencialRepository;
    private readonly ITbCredencialesSyncService _syncService;

    public DeleteCredencialesAccesoCommandHandler(
        IUnitOfWork unitOfWork,
        ICredencialesAccesoRepository credencialRepository,
        ITbCredencialesSyncService syncService)
    {
        _unitOfWork = unitOfWork;
        _credencialRepository = credencialRepository;
        _syncService = syncService;
    }

    public async Task<Result<bool>> Handle(DeleteCredencialesAccesoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var credencial = await _credencialRepository.GetById(request.CredencialId);

            if (credencial == null)
            {
                return Result<bool>.Failure($"Credencial con ID {request.CredencialId} no encontrada.");
            }

            var reservaId = credencial.ReservaId;

            await _credencialRepository.DeleteAsync(credencial, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (reservaId.HasValue)
                await _syncService.SyncByReservaIdAsync(reservaId.Value, cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error al eliminar la credencial de acceso: {ex.Message}");
        }
    }
}
