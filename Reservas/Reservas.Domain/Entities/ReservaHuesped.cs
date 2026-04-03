#nullable disable
using System;

namespace Reservas.Domain.Entites;

public partial class ReservaHuesped
{
    public int ReservaId { get; set; }

    public int HuespedId { get; set; }

    public bool PuedeCrearActividadesRecreativas { get; set; }

    public bool PuedeDesbloquearCerradura { get; set; }

    public DateTime FechaAgregado { get; set; }

    public virtual Reserva Reserva { get; set; }
}
