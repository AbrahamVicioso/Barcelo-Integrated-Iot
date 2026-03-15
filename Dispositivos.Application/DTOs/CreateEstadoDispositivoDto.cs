namespace Dispositivos.Application.DTOs;

public class CreateEstadoDispositivoDto
{
    public int EstadoDispositivoId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}
