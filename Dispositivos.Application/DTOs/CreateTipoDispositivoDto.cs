namespace Dispositivos.Application.DTOs;

public class CreateTipoDispositivoDto
{
    public int TipoDispositivoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
