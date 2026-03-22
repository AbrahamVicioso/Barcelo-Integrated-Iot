namespace Reservas.Domain.Entities;

public class CheckOut
{
    public int CheckOutId { get; set; }

    public int ReservaId { get; set; }

    public DateTime FechaCheckOut { get; set; }

    public virtual Reservas.Domain.Entites.Reserva Reserva { get; set; }
}
