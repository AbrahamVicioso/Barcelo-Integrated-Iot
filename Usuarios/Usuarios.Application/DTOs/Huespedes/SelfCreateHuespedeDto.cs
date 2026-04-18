namespace Usuarios.Application.DTOs.Huespedes;

public class SelfCreateHuespedeDto
{
    public string NombreCompleto { get; set; } = string.Empty;
    public int TipoDocumentoId { get; set; }
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Nacionalidad { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string? ContactoEmergencia { get; set; }
    public string? TelefonoEmergencia { get; set; }
    public string? PreferenciasAlimentarias { get; set; }
    public string? NotasEspeciales { get; set; }
}
