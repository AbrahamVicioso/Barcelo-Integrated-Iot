namespace Reservas.Application.DTOs.Analytics;

public class OcupacionPorHotelDto
{
    public int HotelId { get; set; }
    public string HotelNombre { get; set; } = string.Empty;
    public decimal Ocupacion { get; set; }
}
