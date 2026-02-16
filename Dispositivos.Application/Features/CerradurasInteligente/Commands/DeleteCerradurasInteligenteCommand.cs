using MediatR;
using Dispositivos.Application.Common;

namespace Dispositivos.Application.Features.CerradurasInteligente.Commands;

public class DeleteCerradurasInteligenteCommand : IRequest<Result<bool>>
{
    public int CerraduraId { get; set; }
}
