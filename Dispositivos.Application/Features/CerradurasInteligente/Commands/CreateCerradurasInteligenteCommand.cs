using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.CerradurasInteligente.Commands;

public class CreateCerradurasInteligenteCommand : IRequest<Result<int>>
{
    public CreateCerradurasInteligenteDto Cerradura { get; set; } = new();
}
