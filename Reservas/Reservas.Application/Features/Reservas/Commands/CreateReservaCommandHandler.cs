using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Domain.Entites;
using Reservas.Application.Interfaces;



namespace Reservas.Application.Features.Reservas.Commands;

public class CreateReservaCommandHandler : IRequestHandler<CreateReservaCommand, Result<ReservaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEmailRepository _emailRepository;
    private readonly IHuespedRepository _huespedRepository;

    public CreateReservaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IEmailRepository repository, IHuespedRepository huespedRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        this._emailRepository = repository;
        this._huespedRepository = huespedRepository;
    }

    public async Task<Result<ReservaDto>> Handle(CreateReservaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var reserva = new Reserva {
                FechaCheckIn = request.FechaCheckIn,
                FechaCheckOut = request.FechaCheckOut,
                HuespedId = request.HuespedId,
                HabitacionId = request.HabitacionId,
                
            };

            // Generate unique reservation number
            reserva.NumeroReserva = $"RES-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            reserva.Estado = "Pendiente";
            reserva.FechaCreacion = DateTime.UtcNow;

            await _unitOfWork.Reservas.AddAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var email = await _huespedRepository.GetHuespedIdByEmail(reserva.HuespedId);
             
            await _emailRepository.SendEmailAsync(email, reserva.NumeroReserva, "HOLA" );

            var reservaDto = _mapper.Map<ReservaDto>(reserva);
            return Result<ReservaDto>.Success(reservaDto);
        }
        catch (Exception ex)
        {
            return Result<ReservaDto>.Failure($"Error al crear la reserva: {ex.Message}");
        }
    }
}
