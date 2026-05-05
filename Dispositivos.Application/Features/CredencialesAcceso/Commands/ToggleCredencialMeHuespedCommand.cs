using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.CredencialesAcceso.Commands;

public class ToggleCredencialMeHuespedCommand : IRequest<Result<CredencialesAccesoDto>>
{
    public int CredencialId { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
}
