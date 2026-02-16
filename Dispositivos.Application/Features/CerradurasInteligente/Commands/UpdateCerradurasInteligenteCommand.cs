using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.CerradurasInteligente.Commands;

public class UpdateCerradurasInteligenteCommand : IRequest<Result<CerradurasInteligenteDto>>
{
    public UpdateCerradurasInteligenteDto Cerradura { get; set; } = new();
}
