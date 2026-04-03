namespace Reservas.Application.DTOs;

public class ReservaHuespedItemDto
{
    public int HuespedId { get; set; }
    public bool PuedeCrearActividadesRecreativas { get; set; }
    public bool PuedeDesbloquearCerradura { get; set; }
}
