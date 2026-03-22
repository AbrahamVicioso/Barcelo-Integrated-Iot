using Reservas.Domain.Entities;

namespace Reservas.Application.Interfaces;

public interface ICheckOutRepository : IGenericRepository<CheckOut>
{
    Task<CheckOut?> GetByReservaIdAsync(int reservaId, CancellationToken cancellationToken = default);
    Task<bool> ExisteCheckOutParaReservaAsync(int reservaId, CancellationToken cancellationToken = default);
}
