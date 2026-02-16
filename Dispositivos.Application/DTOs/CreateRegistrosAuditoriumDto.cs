namespace Dispositivos.Application.DTOs;

public class CreateRegistrosAuditoriumDto
{
    public DateTime Fecha { get; set; }
    public string? Accion { get; set; }
    public string? TablaAfectada { get; set; }
    public int? RegistroId { get; set; }
    public string? Usuario { get; set; }
    public string? Detalles { get; set; }
}
