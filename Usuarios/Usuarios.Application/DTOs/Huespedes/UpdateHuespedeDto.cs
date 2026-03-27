namespace Usuarios.Application.DTOs.Huespedes;

public class UpdateHuespedeDto
{
    public string CorreoElectronico { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? ContactoEmergencia { get; set; }
    public string? TelefonoEmergencia { get; set; }
    public bool EsVip { get; set; }
    public string? PreferenciasAlimentarias { get; set; }
    public string? NotasEspeciales { get; set; }
}
