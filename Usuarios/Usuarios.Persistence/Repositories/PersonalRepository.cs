using Microsoft.EntityFrameworkCore;
using Usuarios.Domain.Entities;
using Usuarios.Domain.Interfaces;
using Usuarios.Persistence.Data;

namespace Usuarios.Persistence.Repositories;

public class PersonalRepository : GenericRepository<Personal>, IPersonalRepository
{
    public PersonalRepository(BarceloIoTSystemContext context) : base(context)
    {
    }

    public async Task<Personal?> GetByUsuarioIdAsync(string usuarioId)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);
    }

    public async Task<Personal?> GetByNumeroEmpleadoAsync(string numeroEmpleado)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.NumeroEmpleado == numeroEmpleado);
    }

    public async Task<IEnumerable<Personal>> GetByHotelIdAsync(int hotelId)
    {
        return await _dbSet.Where(p => p.HotelId == hotelId).ToListAsync();
    }

    public async Task<IEnumerable<Personal>> GetByDepartamentoAsync(string departamento)
    {
        return await _dbSet.Where(p => p.Departamento == departamento).ToListAsync();
    }

    public async Task<IEnumerable<Personal>> GetPersonalActivoAsync()
    {
        return await _dbSet.Where(p => p.EstaActivo).ToListAsync();
    }

    public async Task<IEnumerable<Personal>> GetBySupervisorAsync(int supervisorId)
    {
        return await _dbSet.Where(p => p.Supervisor == supervisorId).ToListAsync();
    }

    public async Task<Personal?> GetWithPermisosAsync(int personalId)
    {
        return await _dbSet
            .Include(p => p.PermisosPersonals)
            .FirstOrDefaultAsync(p => p.PersonalId == personalId);
    }
}
