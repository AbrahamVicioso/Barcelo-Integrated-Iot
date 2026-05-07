using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.Historial.Queries;

public class GetHistorialReservasMeQueryHandler : IRequestHandler<GetHistorialReservasMeQuery, Result<PagedResult<ReservaDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHuespedRepository _huespedRepository;
    private readonly IMapper _mapper;

    public GetHistorialReservasMeQueryHandler(
        IUnitOfWork unitOfWork,
        IHuespedRepository huespedRepository,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _huespedRepository = huespedRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<ReservaDto>>> Handle(GetHistorialReservasMeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var huespedId = await _huespedRepository.GetHuespedIdByUserIdAsync(request.UsuarioId, cancellationToken);
            if (huespedId == null)
                return Result<PagedResult<ReservaDto>>.NotFound("Huésped no encontrado para este usuario.");

            var reservas = await _unitOfWork.Reservas.GetReservasByHuespedIdAsync(huespedId.Value, cancellationToken);

            var ordenadas = reservas
                .OrderByDescending(r => r.FechaCheckIn)
                .ToList();

            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize;
            var totalCount = ordenadas.Count;
            var items = ordenadas.Skip((page - 1) * pageSize).Take(pageSize);
            var dtos = _mapper.Map<IEnumerable<ReservaDto>>(items);

            return Result<PagedResult<ReservaDto>>.Success(new PagedResult<ReservaDto>
            {
                Items = dtos,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            });
        }
        catch (Exception ex)
        {
            return Result<PagedResult<ReservaDto>>.Failure($"Error al obtener el historial de reservas: {ex.Message}");
        }
    }
}

public class GetHistorialActividadesMeQueryHandler : IRequestHandler<GetHistorialActividadesMeQuery, Result<PagedResult<ReservaActividadDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHuespedRepository _huespedRepository;
    private readonly IMapper _mapper;

    public GetHistorialActividadesMeQueryHandler(
        IUnitOfWork unitOfWork,
        IHuespedRepository huespedRepository,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _huespedRepository = huespedRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<ReservaActividadDto>>> Handle(GetHistorialActividadesMeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var huespedId = await _huespedRepository.GetHuespedIdByUserIdAsync(request.UsuarioId, cancellationToken);
            if (huespedId == null)
                return Result<PagedResult<ReservaActividadDto>>.NotFound("Huésped no encontrado para este usuario.");

            var actividades = await _unitOfWork.ReservasActividades.GetReservasByHuespedIdAsync(huespedId.Value, cancellationToken);

            var ordenadas = actividades
                .OrderByDescending(r => r.FechaReserva.Date.Add(r.HoraReserva))
                .ToList();

            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize;
            var totalCount = ordenadas.Count;
            var items = ordenadas.Skip((page - 1) * pageSize).Take(pageSize);
            var dtos = _mapper.Map<IEnumerable<ReservaActividadDto>>(items);

            return Result<PagedResult<ReservaActividadDto>>.Success(new PagedResult<ReservaActividadDto>
            {
                Items = dtos,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            });
        }
        catch (Exception ex)
        {
            return Result<PagedResult<ReservaActividadDto>>.Failure($"Error al obtener el historial de actividades: {ex.Message}");
        }
    }
}
