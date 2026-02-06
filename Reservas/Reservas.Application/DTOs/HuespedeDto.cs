using System.Text.Json.Serialization;

namespace Reservas.Application.DTOs;

public class HuespedeDto
{
    [JsonPropertyName("huespedId")]
    public int HuespedId { get; set; }

    [JsonPropertyName("usuarioId")]
    public string UsuarioId { get; set; } = string.Empty;

    [JsonPropertyName("nombreCompleto")]
    public string NombreCompleto { get; set; } = string.Empty;

    [JsonPropertyName("tipoDocumento")]
    public string TipoDocumento { get; set; } = string.Empty;

    [JsonPropertyName("numeroDocumento")]
    public string NumeroDocumento { get; set; } = string.Empty;

    [JsonPropertyName("nacionalidad")]
    public string Nacionalidad { get; set; } = string.Empty;

    [JsonPropertyName("fechaNacimiento")]
    public DateTime FechaNacimiento { get; set; }

    [JsonPropertyName("contactoEmergencia")]
    public string? ContactoEmergencia { get; set; }

    [JsonPropertyName("telefonoEmergencia")]
    public string? TelefonoEmergencia { get; set; }

    [JsonPropertyName("esVip")]
    public bool EsVip { get; set; }

    [JsonPropertyName("fechaRegistro")]
    public DateTime FechaRegistro { get; set; }

    [JsonPropertyName("preferenciasAlimentarias")]
    public string? PreferenciasAlimentarias { get; set; }

    [JsonPropertyName("notasEspeciales")]
    public string? NotasEspeciales { get; set; }

    [JsonPropertyName("correoElectronico")]
    public string? CorreoElectronico { get; set; }
}
