using Microsoft.EntityFrameworkCore;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entities;
using Reservas.Persistence.Data;

namespace Reservas.Persistence.Repositories;

public class CheckOutRepository : GenericRepository<CheckOut>, ICheckOutRepository
{
    public CheckOutRepository(BarceloReservasContext context) : base(context)
    {
    }

    public async Task<CheckOut?> GetByReservaIdAsync(int reservaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Reserva)
            .FirstOrDefaultAsync(c => c.ReservaId == reservaId, cancellationToken);
    }

    public async Task<bool> ExisteCheckOutParaReservaAsync(int reservaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(c => c.ReservaId == reservaId, cancellationToken);
    }
}
