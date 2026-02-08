using Dispositivos.Domain.Entities;

namespace Dispositivos.Application.Interfaces;

public interface IDispositivoRepository
{
    Task<IEnumerable<Dispositivo>> GetAll();
    Task<Dispositivo?> GetById(int id);
    Task<IEnumerable<Dispositivo>> GetByHotelId(int hotelId);
    Task<IEnumerable<Dispositivo>> GetByTipoDispositivo(string tipoDispositivo);
    Task<IEnumerable<Dispositivo>> GetByEstaEnLinea(bool estaEnLinea);
    Task AddAsync(Dispositivo dispositivo, CancellationToken cancellationToken = default);
    Task UpdateAsync(Dispositivo dispositivo, CancellationToken cancellationToken = default);
    Task DeleteAsync(Dispositivo dispositivo, CancellationToken cancellationToken = default);
}
