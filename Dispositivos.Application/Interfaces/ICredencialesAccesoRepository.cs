using Dispositivos.Domain.Entities;

namespace Dispositivos.Application.Interfaces;

public interface ICredencialesAccesoRepository
{
    Task<IEnumerable<CredencialesAcceso>> GetAll();
    Task<CredencialesAcceso?> GetById(int id);
    Task<IEnumerable<CredencialesAcceso>> GetByHuespedId(int huespedId);
    Task<IEnumerable<CredencialesAcceso>> GetByReservaIdAsync(int reservaId);
    Task<IEnumerable<CredencialesAcceso>> GetByReservaActividadIdAsync(int reservaActividadId);
    Task<IEnumerable<CredencialesAcceso>> GetByPersonalId(int personalId);
    Task<IEnumerable<CredencialesAcceso>> GetByEstaActiva(bool estaActiva);
    Task<IEnumerable<CredencialesAcceso>> GetByTipoCredencial(string tipoCredencial);
    Task<CredencialesAcceso?> GetByCodigoPin(string codigoPin);
    Task<CredencialesAcceso?> GetByPinAndFechas(string codigoPin, DateTime fechaActivacion, DateTime fechaExpiracion);
    Task AddAsync(CredencialesAcceso credencial, CancellationToken cancellationToken = default);
    Task UpdateAsync(CredencialesAcceso credencial, CancellationToken cancellationToken = default);
    Task DeleteAsync(CredencialesAcceso credencial, CancellationToken cancellationToken = default);
}
