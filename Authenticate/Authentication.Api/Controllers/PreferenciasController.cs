using Authentication.Api.DTOs;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification.Domain.Data;
using Notification.Kafka.Data;

namespace Authentication.Api.Controllers;

[ApiController]
[Route("me/notificaciones")]
[Authorize]
public class PreferenciasController : ControllerBase
{
    private readonly NotificacionDbContext _context;
    private readonly UserManager<User> _userManager;

    public PreferenciasController(
        NotificacionDbContext context,
        UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /// <summary>
    /// Obtener preferencias de notificaciones del usuario autenticado
    /// </summary>
    [HttpGet("preferencias")]
    public async Task<ActionResult<PreferenciasResponseDto>> GetPreferencias()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var prefs = await _context.PreferenciasNotificacion
            .FirstOrDefaultAsync(p => p.UsuarioId == user.Id);

        if (prefs == null)
        {
            // Return defaults if not exists
            return Ok(new PreferenciasResponseDto
            {
                PreferenciaId = 0,
                HabilitarNotificacionesPush = true,
                HabilitarNotificacionesEmail = true,
                HabilitarNotificacionesSMS = false,
                NotificarAccesoPersonal = true,
                NotificarRecordatorioActividad = true,
                NotificarPromocionesOfertas = true,
                NotificarReservas = true,
                NotificarCredenciales = true,
                NotificarCheckIn = true,
                NotificarCuentaCreada = true,
                NotificarConfirmacionEmail = true,
                NotificarRestablecerPassword = true,
                HorarioNoMolestar = false,
                HoraInicioNoMolestar = null,
                HoraFinNoMolestar = null,
                FechaActualizacion = DateTime.Now
            });
        }

        return Ok(MapToDto(prefs));
    }

    /// <summary>
    /// Actualizar preferencias de notificaciones del usuario autenticado
    /// </summary>
    [HttpPut("preferencias")]
    public async Task<ActionResult<PreferenciasResponseDto>> UpdatePreferencias([FromBody] UpdatePreferenciasDto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Validar horario no molestar
        if (dto.HorarioNoMolestar)
        {
            if (dto.HoraInicioNoMolestar == null || dto.HoraFinNoMolestar == null)
            {
                return BadRequest(new { error = "HoraInicioNoMolestar y HoraFinNoMolestar son requeridas cuando HorarioNoMolestar está habilitado" });
            }
        }

        var prefs = await _context.PreferenciasNotificacion
            .FirstOrDefaultAsync(p => p.UsuarioId == user.Id);

        if (prefs == null)
        {
            prefs = new PreferenciaNotificacion
            {
                UsuarioId = user.Id,
                FechaActualizacion = DateTime.Now
            };
            _context.PreferenciasNotificacion.Add(prefs);
        }

        // Update fields
        prefs.HabilitarNotificacionesPush = dto.HabilitarNotificacionesPush;
        prefs.HabilitarNotificacionesEmail = dto.HabilitarNotificacionesEmail;
        prefs.HabilitarNotificacionesSMS = dto.HabilitarNotificacionesSMS;
        prefs.NotificarAccesoPersonal = dto.NotificarAccesoPersonal;
        prefs.NotificarRecordatorioActividad = dto.NotificarRecordatorioActividad;
        prefs.NotificarPromocionesOfertas = dto.NotificarPromocionesOfertas;
        prefs.NotificarReservas = dto.NotificarReservas;
        prefs.NotificarCredenciales = dto.NotificarCredenciales;
        prefs.NotificarCheckIn = dto.NotificarCheckIn;
        prefs.NotificarCuentaCreada = dto.NotificarCuentaCreada;
        prefs.NotificarConfirmacionEmail = dto.NotificarConfirmacionEmail;
        prefs.NotificarRestablecerPassword = dto.NotificarRestablecerPassword;
        prefs.HorarioNoMolestar = dto.HorarioNoMolestar;
        prefs.HoraInicioNoMolestar = dto.HoraInicioNoMolestar;
        prefs.HoraFinNoMolestar = dto.HoraFinNoMolestar;
        prefs.FechaActualizacion = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(MapToDto(prefs));
    }

    /// <summary>
    /// Obtener historial de notificaciones del usuario autenticado
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<NotificacionResponseDto>>> GetNotificaciones([FromQuery] PaginationParams @params)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var query = _context.Notificaciones
            .Where(n => n.UsuarioId == user.Id)
            .OrderByDescending(n => n.FechaEnvio);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((@params.Page - 1) * @params.PageSize)
            .Take(@params.PageSize)
            .Select(n => new NotificacionResponseDto
            {
                NotificacionId = n.NotificacionId,
                TipoNotificacion = n.TipoNotificacion,
                Titulo = n.Titulo,
                Mensaje = n.Mensaje,
                Prioridad = n.Prioridad,
                FueLeida = n.FueLeida,
                FechaEnvio = n.FechaEnvio,
                FechaLectura = n.FechaLectura,
                TipoEntidadRelacionada = n.TipoEntidadRelacionada,
                EntidadRelacionadaId = n.EntidadRelacionadaId,
                CanalEnvio = n.CanalEnvio,
                EstadoEnvio = n.EstadoEnvio
            })
            .ToListAsync();

        return Ok(Authentication.Api.DTOs.PagedResult<NotificacionResponseDto>.Create(items, @params.Page, @params.PageSize, totalCount));
    }

    /// <summary>
    /// Marcar notificación como leída
    /// </summary>
    [HttpPut("{id}/leida")]
    public async Task<IActionResult> MarcarLeida(long id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var notificacion = await _context.Notificaciones
            .FirstOrDefaultAsync(n => n.NotificacionId == id && n.UsuarioId == user.Id);

        if (notificacion == null) return NotFound();

        notificacion.FueLeida = true;
        notificacion.FechaLectura = DateTime.Now;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Eliminar notificación del historial
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarNotificacion(long id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var notificacion = await _context.Notificaciones
            .FirstOrDefaultAsync(n => n.NotificacionId == id && n.UsuarioId == user.Id);

        if (notificacion == null) return NotFound();

        _context.Notificaciones.Remove(notificacion);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private PreferenciasResponseDto MapToDto(PreferenciaNotificacion prefs)
    {
        return new PreferenciasResponseDto
        {
            PreferenciaId = prefs.PreferenciaId,
            HabilitarNotificacionesPush = prefs.HabilitarNotificacionesPush,
            HabilitarNotificacionesEmail = prefs.HabilitarNotificacionesEmail,
            HabilitarNotificacionesSMS = prefs.HabilitarNotificacionesSMS,
            NotificarAccesoPersonal = prefs.NotificarAccesoPersonal,
            NotificarRecordatorioActividad = prefs.NotificarRecordatorioActividad,
            NotificarPromocionesOfertas = prefs.NotificarPromocionesOfertas,
            NotificarReservas = prefs.NotificarReservas,
            NotificarCredenciales = prefs.NotificarCredenciales,
            NotificarCheckIn = prefs.NotificarCheckIn,
            NotificarCuentaCreada = prefs.NotificarCuentaCreada,
            NotificarConfirmacionEmail = prefs.NotificarConfirmacionEmail,
            NotificarRestablecerPassword = prefs.NotificarRestablecerPassword,
            HorarioNoMolestar = prefs.HorarioNoMolestar,
            HoraInicioNoMolestar = prefs.HoraInicioNoMolestar,
            HoraFinNoMolestar = prefs.HoraFinNoMolestar,
            FechaActualizacion = prefs.FechaActualizacion
        };
    }
}
