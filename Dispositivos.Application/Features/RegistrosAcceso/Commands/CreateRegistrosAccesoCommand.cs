using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.RegistrosAcceso.Commands;

public class CreateRegistrosAccesoCommand : IRequest<Result<long>>
{
    public CreateRegistrosAccesoDto Registro { get; set; } = new();
}
