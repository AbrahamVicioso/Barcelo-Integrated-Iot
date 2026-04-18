using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;
using System.Linq;

namespace Reservas.Application.Features.Reservas.Queries;

public class GetReservaCredencialesQueryHandler : IRequestHandler<GetReservaCredencialesQuery, Result<IEnumerable<CredencialHuespedDto>>>
{
    private readonly IUsuariosApiService _usuariosApiService;
    private readonly IReservaRepository _reservaRepository;
    private readonly ICredencialesAccesoService _credencialesAccesoService;

    public GetReservaCredencialesQueryHandler(
        IUsuariosApiService usuariosApiService,
        IReservaRepository reservaRepository,
        ICredencialesAccesoService credencialesAccesoService)
    {
        _usuariosApiService = usuariosApiService;
        _reservaRepository = reservaRepository;
        _credencialesAccesoService = credencialesAccesoService;
    }

    public async Task<Result<IEnumerable<CredencialHuespedDto>>> Handle(GetReservaCredencialesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Get the Huesped from the Usuarios API using the UserId from JWT
            var huesped = await _usuariosApiService.GetHuespedByUsuarioIdAsync(request.UserId, cancellationToken);
            if (huesped == null)
            {
                return Result<IEnumerable<CredencialHuespedDto>>.Failure("No se encontró ningún huésped asociado a este usuario");
            }

            // 2. Get the reservation and include guests
            var reserva = await _reservaRepository.GetByIdAsync(request.ReservaId, cancellationToken);
            if (reserva == null)
            {
                return Result<IEnumerable<CredencialHuespedDto>>.NotFound($"Reserva con ID {request.ReservaId} no encontrada");
            }

            // 3. Check authorization
            bool isAuthorized = false;

            // Case A: Main Guest
            if (reserva.HuespedId == huesped.HuespedId)
            {
                isAuthorized = true;
            }
            // Case B: Additional Guest (ReservaHuesped)
            else 
            {
                var reservaHuesped = reserva.ReservaHuespedes.FirstOrDefault(rh => rh.HuespedId == huesped.HuespedId);
                if (reservaHuesped != null)
                {
                    // Check if they have permission to unlock (view credentials)
                    if (reservaHuesped.PuedeDesbloquearCerradura)
                    {
                        isAuthorized = true;
                    }
                    else
                    {
                        return Result<IEnumerable<CredencialHuespedDto>>.Failure("No tienes permisos para ver las credenciales de esta reserva");
                    }
                }
            }

            if (!isAuthorized)
            {
                return Result<IEnumerable<CredencialHuespedDto>>.Failure("No eres parte de esta reserva");
            }

            // 4. Fetch credentials
            var credenciales = await _credencialesAccesoService.GetCredencialesForHuespedAsync(request.ReservaId, huesped.HuespedId, cancellationToken);
            
            return Result<IEnumerable<CredencialHuespedDto>>.Success(credenciales);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<CredencialHuespedDto>>.Failure($"Error al obtener las credenciales: {ex.Message}");
        }
    }
}
