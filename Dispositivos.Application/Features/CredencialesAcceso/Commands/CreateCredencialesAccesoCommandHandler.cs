using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;
using CredencialEntity = Dispositivos.Domain.Entities.CredencialesAcceso;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Dispositivos.Application.Features.CredencialesAcceso.Commands;

public class CreateCredencialesAccesoCommandHandler : IRequestHandler<CreateCredencialesAccesoCommand, Result<int>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICredencialesAccesoRepository _credencialRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateCredencialesAccesoCommandHandler(
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ICredencialesAccesoRepository credencialRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _credencialRepository = credencialRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GenerarHash(string texto)
{
    using var sha256 = SHA256.Create();
    var bytes = Encoding.UTF8.GetBytes(texto);
    var hash = sha256.ComputeHash(bytes);
    return Convert.ToBase64String(hash);
}
    public async Task<Result<int>> Handle(CreateCredencialesAccesoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Credencial.FechaExpiracion <= request.Credencial.FechaActivacion)
                return Result<int>.Failure("La fecha de expiración debe ser posterior a la fecha de activación.");

            if (request.Credencial.FechaActivacion < DateTime.UtcNow.Date)
                return Result<int>.Failure("La fecha de activación no puede ser en el pasado.");

            var user = _httpContextAccessor.HttpContext?.User;
            var userId = user?.FindFirst("nameid")?.Value
                      ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Result<int>.Failure("No se pudo identificar al usuario autenticado.");

            var credencial = _mapper.Map<CredencialEntity>(request.Credencial);
            credencial.FechaCreacion = DateTime.UtcNow;
            credencial.CreadoPor = userId;
            credencial.HashPin = GenerarHash(credencial.CodigoPin);

            await _credencialRepository.AddAsync(credencial, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(credencial.CredencialId);
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")
        {
            var inner = ex.InnerException?.Message ?? ex.Message;
            if (inner.Contains("CHK_Credenciales_Fechas"))
                return Result<int>.Failure("La fecha de expiración debe ser posterior a la fecha de activación.");
            return Result<int>.Failure($"Error de base de datos al crear la credencial: {inner}");
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error al crear la credencial de acceso: {ex.Message}");
        }
    }
}
