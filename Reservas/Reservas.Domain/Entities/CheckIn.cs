namespace Reservas.Domain.Entities;

public class CheckIn
{
    public int CheckInId { get; set; }

    public int ReservaId { get; set; }

    public string NombreCompleto { get; set; }

    public string Email { get; set; }

    public string Telefono { get; set; }

    public DateTime FechaCheckIn { get; set; }

    public string DireccionIp { get; set; }

    public string Estado { get; set; }

    public virtual Reservas.Domain.Entites.Reserva Reserva { get; set; }
}
