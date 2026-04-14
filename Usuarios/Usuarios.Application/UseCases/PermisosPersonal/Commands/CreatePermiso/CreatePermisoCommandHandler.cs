using System.Security.Claims;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Usuarios.Application.DTOs.PermisosPersonal;
using Usuarios.Application.Exceptions;
using Usuarios.Application.Interfaces;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.PermisosPersonal.Commands.CreatePermiso;

public class CreatePermisoCommandHandler : IRequestHandler<CreatePermisoCommand, PermisosPersonalDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPermisoHabitacionSyncProducer _syncProducer;

    public CreatePermisoCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        IPermisoHabitacionSyncProducer syncProducer)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _syncProducer = syncProducer;
    }

    public async Task<PermisosPersonalDto> Handle(CreatePermisoCommand request, CancellationToken cancellationToken)
    {
        var personal = await _unitOfWork.Personal.GetByIdAsync(request.Permiso.PersonalId);
        if (personal == null)
        {
            throw new NotFoundException("El personal especificado no existe");
        }

        if (!personal.EstaActivo)
        {
            throw new BusinessException("No se pueden otorgar permisos a personal inactivo");
        }

        if (request.Permiso.EsTemporal && !request.Permiso.FechaExpiracion.HasValue)
        {
            throw new BusinessException("Los permisos temporales deben tener fecha de expiración");
        }

        var user = _httpContextAccessor.HttpContext?.User;
        // Con MapInboundClaims=false el claim queda como "nameid" (nombre corto JWT),
        // no como la URI larga de ClaimTypes.NameIdentifier
        var userId = user?.FindFirst("nameid")?.Value
                  ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new BusinessException("No se pudo identificar al usuario autenticado.");

        var permiso = _mapper.Map<Domain.Entities.PermisosPersonal>(request.Permiso);
        permiso.FechaOtorgamiento = DateTime.UtcNow;
        permiso.EstaActivo = true;
        permiso.OtorgadoPor = userId;

        try
        {
            var createdPermiso = await _unitOfWork.PermisosPersonal.AddAsync(permiso);
            await _unitOfWork.SaveChangesAsync();

            // Notify Dispositivos to sync ThingsBoard credentials for this habitacion
            if (request.Permiso.HabitacionId.HasValue)
                await _syncProducer.PublishAsync(request.Permiso.HabitacionId.Value, cancellationToken);

            return _mapper.Map<PermisosPersonalDto>(createdPermiso);
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")
        {
            var inner = ex.InnerException?.Message ?? ex.Message;
            if (inner.Contains("FK_PermisosPersonal_OtorgadoPor"))
                throw new BusinessException("El usuario especificado en OtorgadoPor no existe.");
            if (inner.Contains("FK_PermisosPersonal_Habitaciones"))
                throw new BusinessException($"La habitación {request.Permiso.HabitacionId} no existe.");
            if (inner.Contains("FK_PermisosPersonal_ActividadesRecreativas"))
                throw new BusinessException($"La actividad recreativa {request.Permiso.ActividadId} no existe.");
            if (inner.Contains("FK_PermisosPersonal_Personal"))
                throw new NotFoundException("El personal especificado no existe.");
            throw new BusinessException($"Error de base de datos: {inner}");
        }
    }
}
