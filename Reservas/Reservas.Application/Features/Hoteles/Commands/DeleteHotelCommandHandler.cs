using MediatR;
using Reservas.Application.Common;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.Hoteles.Commands;

public class DeleteHotelCommandHandler : IRequestHandler<DeleteHotelCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteHotelCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteHotelCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var hotel = await _unitOfWork.Hoteles.GetById(request.HotelId);

            if (hotel == null)
            {
                return Result<bool>.Failure($"Hotel con ID {request.HotelId} no encontrado.");
            }

            await _unitOfWork.Hoteles.DeleteAsync(hotel, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error al desactivar el hotel: {ex.Message}");
        }
    }
}
