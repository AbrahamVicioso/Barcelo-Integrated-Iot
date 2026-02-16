using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.CerradurasInteligente.Queries;

public class GetAllCerradurasInteligenteQuery : IRequest<Result<IEnumerable<CerradurasInteligenteDto>>>
{
}
