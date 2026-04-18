using MediatR;
using Usuarios.Application.DTOs.Huespedes;

namespace Usuarios.Application.UseCases.Huespedes.Commands.CreateHuespedeMe;

public record CreateHuespedeMeCommand(string UsuarioId, SelfCreateHuespedeDto Huespede) : IRequest<HuespedeDto>;
