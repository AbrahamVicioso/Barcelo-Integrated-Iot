using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.CerradurasInteligente.Queries;

public class GetCerradurasInteligenteByIdQuery : IRequest<Result<CerradurasInteligenteDto>>
{
    public int CerraduraId { get; set; }
}
