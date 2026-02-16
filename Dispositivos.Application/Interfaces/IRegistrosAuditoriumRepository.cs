using Dispositivos.Domain.Entities;

namespace Dispositivos.Application.Interfaces;

public interface IRegistrosAuditoriumRepository
{
    Task<IEnumerable<RegistrosAuditorium>> GetAllAsync();
    Task<RegistrosAuditorium?> GetByIdAsync(int id);
    Task<IEnumerable<RegistrosAuditorium>> GetByUsuarioIdAsync(string usuarioId);
    Task<IEnumerable<RegistrosAuditorium>> GetByTipoEntidadAsync(string tipoEntidad);
    Task<IEnumerable<RegistrosAuditorium>> GetByHotelIdAsync(int hotelId);
    Task<RegistrosAuditorium> AddAsync(RegistrosAuditorium registro, CancellationToken cancellationToken = default);
    Task UpdateAsync(RegistrosAuditorium registro, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
