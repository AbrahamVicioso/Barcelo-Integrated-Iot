using Reservas.Domain.Entities;

namespace Reservas.Application.Interfaces;

public interface ICheckInRepository : IGenericRepository<CheckIn>
{
    Task<CheckIn?> GetByReservaIdAsync(int reservaId, CancellationToken cancellationToken = default);
    Task<bool> ExisteCheckInParaReservaAsync(int reservaId, CancellationToken cancellationToken = default);
}
