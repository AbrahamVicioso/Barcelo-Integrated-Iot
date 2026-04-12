using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.Reservas.Queries;

public class GetAllReservasQueryHandler : IRequestHandler<GetAllReservasQuery, Result<IEnumerable<ReservaDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUsuariosApiService _usuariosApiService;

    public GetAllReservasQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IUsuariosApiService usuariosApiService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _usuariosApiService = usuariosApiService;
    }

    public async Task<Result<IEnumerable<ReservaDto>>> Handle(GetAllReservasQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var reservas = await _unitOfWork.Reservas.GetAllAsync(cancellationToken);

            if (request.EstadoReservaId.HasValue)
                reservas = reservas.Where(r => r.EstadoReservaId == request.EstadoReservaId.Value);

            // Solapamiento: la reserva aplica si está activa en algún punto del rango.
            // Se compara solo la parte de fecha (.Date) para ignorar la hora enviada por el cliente.
            if (request.FechaInicio.HasValue)
                reservas = reservas.Where(r => r.FechaCheckOut.Date >= request.FechaInicio.Value.Date);

            if (request.FechaFin.HasValue)
                reservas = reservas.Where(r => r.FechaCheckIn.Date <= request.FechaFin.Value.Date);

            var reservasList = reservas.ToList();

            if (!string.IsNullOrWhiteSpace(request.NombreHuesped))
            {
                var huespedIds = reservasList
                    .Select(r => r.HuespedId)
                    .Distinct()
                    .ToList();

                var huespedTasks = huespedIds.ToDictionary(
                    id => id,
                    id => _usuariosApiService.GetHuespedByIdAsync(id, cancellationToken));

                await Task.WhenAll(huespedTasks.Values);

                var huespedNombres = huespedTasks.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Result?.NombreCompleto ?? string.Empty);

                var filtro = request.NombreHuesped.Trim();
                reservasList = reservasList
                    .Where(r => huespedNombres.TryGetValue(r.HuespedId, out var nombre)
                                && nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var reservasDto = _mapper.Map<IEnumerable<ReservaDto>>(reservasList);
            return Result<IEnumerable<ReservaDto>>.Success(reservasDto);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ReservaDto>>.Failure($"Error al obtener las reservas: {ex.Message}");
        }
    }
}
