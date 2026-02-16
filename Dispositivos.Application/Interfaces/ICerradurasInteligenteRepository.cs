using Dispositivos.Domain.Entities;

namespace Dispositivos.Application.Interfaces;

public interface ICerradurasInteligenteRepository
{
    Task<IEnumerable<CerradurasInteligente>> GetAll();
    Task<CerradurasInteligente?> GetById(int id);
    Task<IEnumerable<CerradurasInteligente>> GetByDispositivoId(Guid dispositivoId);
    Task<IEnumerable<CerradurasInteligente>> GetByHabitacionId(int habitacionId);
    Task<IEnumerable<CerradurasInteligente>> GetByEstaActiva(bool estaActiva);
    Task AddAsync(CerradurasInteligente cerradura, CancellationToken cancellationToken = default);
    Task UpdateAsync(CerradurasInteligente cerradura, CancellationToken cancellationToken = default);
    Task DeleteAsync(CerradurasInteligente cerradura, CancellationToken cancellationToken = default);
}
