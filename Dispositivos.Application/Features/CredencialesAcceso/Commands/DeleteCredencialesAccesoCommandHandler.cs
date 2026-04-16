using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Dispositivos.Application.Common;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.CredencialesAcceso.Commands;

public class DeleteCredencialesAccesoCommandHandler : IRequestHandler<DeleteCredencialesAccesoCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICredencialesAccesoRepository _credencialRepository;
    private readonly IServiceScopeFactory _scopeFactory;

    public DeleteCredencialesAccesoCommandHandler(
        IUnitOfWork unitOfWork,
        ICredencialesAccesoRepository credencialRepository,
        IServiceScopeFactory scopeFactory)
    {
        _unitOfWork = unitOfWork;
        _credencialRepository = credencialRepository;
        _scopeFactory = scopeFactory;
    }

    public async Task<Result<bool>> Handle(DeleteCredencialesAccesoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var credencial = await _credencialRepository.GetById(request.CredencialId);

            if (credencial == null)
                return Result<bool>.NotFound($"Credencial con ID {request.CredencialId} no encontrada.");

            var reservaId = credencial.ReservaId;

            await _credencialRepository.DeleteAsync(credencial, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Sync en scope fresco para evitar interferencias del DbContext actual
            if (reservaId.HasValue)
            {
                using var scope = _scopeFactory.CreateScope();
                var syncService = scope.ServiceProvider.GetRequiredService<ITbCredencialesSyncService>();
                await syncService.SyncByReservaIdAsync(reservaId.Value, cancellationToken);
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error al eliminar la credencial de acceso: {ex.Message}");
        }
    }
}
