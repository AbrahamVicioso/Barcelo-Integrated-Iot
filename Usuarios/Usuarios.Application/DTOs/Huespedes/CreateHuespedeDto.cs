namespace Usuarios.Application.DTOs.Huespedes;

public class CreateHuespedeDto
{
    public string UsuarioId { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Nacionalidad { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string? ContactoEmergencia { get; set; }
    public string? TelefonoEmergencia { get; set; }
    public bool EsVip { get; set; }
    public string? PreferenciasAlimentarias { get; set; }
    public string? NotasEspeciales { get; set; }
}
